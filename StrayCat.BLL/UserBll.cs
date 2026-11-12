using System;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class UserBll
    {
        public const int PageSize = 10;

        private readonly Func<string, SysUser> _findUser;
        private readonly Func<int> _countEnabled;
        private readonly Func<UserDal> _storeFactory;
        private UserDal _store;

        public UserBll()
            : this(
                delegate(string name) { return new UserDal().GetByUserName(name); },
                delegate { return new UserDal().CountEnabled(); })
        {
            _storeFactory = delegate { return new UserDal(); };
        }

        public UserBll(string connectionString)
            : this(
                delegate(string name) { return new UserDal(connectionString).GetByUserName(name); },
                delegate { return new UserDal(connectionString).CountEnabled(); })
        {
            _storeFactory = delegate { return new UserDal(connectionString); };
        }

        public UserBll(Func<string, SysUser> findUser)
            : this(findUser, delegate { return 0; })
        {
        }

        public UserBll(Func<string, SysUser> findUser, Func<int> countEnabled)
        {
            _findUser = findUser;
            _countEnabled = countEnabled;
        }

        public OperationResult<CurrentUser> Authenticate(SysUser user, string password)
        {
            if (user == null)
            {
                return OperationResult<CurrentUser>.Fail(Messages.BadLogin);
            }
            if (!user.IsEnabled)
            {
                return OperationResult<CurrentUser>.Fail(Messages.Disabled);
            }
            if (string.IsNullOrEmpty(password))
            {
                return OperationResult<CurrentUser>.Fail(Messages.BadLogin);
            }

            string actual = PasswordHasher.Hash(user.Salt, password);
            string expected = (user.PasswordHash ?? "").Trim();
            if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
            {
                return OperationResult<CurrentUser>.Fail(Messages.BadLogin);
            }

            return OperationResult<CurrentUser>.Ok(new CurrentUser
            {
                UserId = user.UserId,
                UserName = user.UserName,
                RealName = user.RealName,
                Role = user.Role
            });
        }

        public OperationResult<CurrentUser> Login(string userName, string password)
        {
            if (string.IsNullOrWhiteSpace(userName) || string.IsNullOrEmpty(password))
            {
                return OperationResult<CurrentUser>.Fail(Messages.BadLogin);
            }
            SysUser user = _findUser(userName.Trim());
            return Authenticate(user, password);
        }

        public OperationResult<int> Register(string userName, string password, string confirmPassword, string realName, string phone)
        {
            if (!string.Equals(password ?? "", confirmPassword ?? "", StringComparison.Ordinal))
            {
                return OperationResult<int>.Fail(Messages.PasswordMismatch);
            }
            return Create(userName, password, realName, phone, RoleCodes.Staff, true);
        }

        public int CountEnabled()
        {
            return _countEnabled();
        }

        public SysUser Get(int userId)
        {
            return Store.GetById(userId);
        }

        public PagedResult<SysUser> Search(string keyword, int pageIndex)
        {
            if (pageIndex < 1)
            {
                pageIndex = 1;
            }
            PagedResult<SysUser> page = Store.Search(FieldRules.Clean(keyword), pageIndex, PageSize);
            int pages = page.TotalCount == 0 ? 1 : (page.TotalCount + PageSize - 1) / PageSize;
            if (pageIndex > pages)
            {
                pageIndex = pages;
                page = Store.Search(FieldRules.Clean(keyword), pageIndex, PageSize);
            }
            page.PageIndex = pageIndex;
            page.PageSize = PageSize;
            return page;
        }

        public OperationResult<int> Create(string userName, string password, string realName, string phone, byte role, bool enabled)
        {
            userName = FieldRules.Clean(userName);
            realName = FieldRules.Clean(realName);
            phone = FieldRules.Clean(phone);
            string error = FieldRules.First(
                FieldRules.UserName(userName),
                FieldRules.Password(password),
                FieldRules.Required(realName, 50),
                FieldRules.Phone(phone, false),
                ValidRole(role));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            string salt = PasswordHasher.NewSalt();
            string hash = PasswordHasher.Hash(salt, password);
            return Store.Insert(userName, hash, salt, realName, role, phone, enabled);
        }

        public OperationResult<int> Update(int userId, int operatorId, string realName, string phone, byte role, bool enabled, string newPassword)
        {
            realName = FieldRules.Clean(realName);
            phone = FieldRules.Clean(phone);
            string passwordError = string.IsNullOrEmpty(newPassword) ? null : FieldRules.Password(newPassword);
            string error = FieldRules.First(
                FieldRules.Required(realName, 50),
                FieldRules.Phone(phone, false),
                passwordError,
                ValidRole(role));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }

            SysUser existing = Store.GetById(userId);
            if (existing == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!enabled && userId == operatorId)
            {
                return OperationResult<int>.Fail(Messages.CannotDisableSelf);
            }
            if (IsLastAdminChange(existing, role, enabled))
            {
                return OperationResult<int>.Fail(Messages.CannotDisableLastAdmin);
            }

            string hash = null;
            string salt = null;
            if (!string.IsNullOrEmpty(newPassword))
            {
                salt = PasswordHasher.NewSalt();
                hash = PasswordHasher.Hash(salt, newPassword);
            }
            return Store.Update(userId, operatorId, realName, phone, role, enabled, hash, salt);
        }

        public OperationResult<int> SetEnabled(int userId, int operatorId, bool enabled)
        {
            if (!enabled && userId == operatorId)
            {
                return OperationResult<int>.Fail(Messages.CannotDisableSelf);
            }
            SysUser existing = Store.GetById(userId);
            if (existing == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (IsLastAdminChange(existing, existing.Role, enabled))
            {
                return OperationResult<int>.Fail(Messages.CannotDisableLastAdmin);
            }
            return Store.SetEnabled(userId, operatorId, enabled);
        }

        public OperationResult<int> ChangePassword(int userId, string oldPassword, string newPassword)
        {
            string error = FieldRules.Password(newPassword);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            SysUser user = Store.GetById(userId);
            if (user == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            OperationResult<CurrentUser> check = Authenticate(user, oldPassword);
            if (!check.Success)
            {
                return OperationResult<int>.Fail(check.Message);
            }
            string salt = PasswordHasher.NewSalt();
            string hash = PasswordHasher.Hash(salt, newPassword);
            return Store.ChangePassword(userId, hash, salt);
        }

        private bool IsLastAdminChange(SysUser existing, byte newRole, bool enabled)
        {
            if (existing.Role != RoleCodes.Admin || !existing.IsEnabled)
            {
                return false;
            }
            if (newRole == RoleCodes.Admin && enabled)
            {
                return false;
            }
            return Store.CountEnabledAdmins() <= 1;
        }

        private static string ValidRole(byte role)
        {
            if (role == RoleCodes.Admin || role == RoleCodes.Staff)
            {
                return null;
            }
            return Messages.RequiredField;
        }

        private UserDal Store
        {
            get
            {
                if (_store == null)
                {
                    if (_storeFactory == null)
                    {
                        throw new InvalidOperationException("当前业务对象不能保存账号。");
                    }
                    _store = _storeFactory();
                }
                return _store;
            }
        }
    }
}
