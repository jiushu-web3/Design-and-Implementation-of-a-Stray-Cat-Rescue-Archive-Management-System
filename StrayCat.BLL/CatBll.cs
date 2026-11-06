using System.Collections.Generic;
using StrayCat.DAL;
using StrayCat.Model;

namespace StrayCat.BLL
{
    public class CatBll
    {
        public const int PageSize = 10;

        private readonly CatDal _cats;
        private readonly MedicalDal _medical;
        private readonly AdoptionDal _adoptions;

        public CatBll()
            : this(new DbHelper())
        {
        }

        public CatBll(string connectionString)
            : this(new DbHelper(connectionString))
        {
        }

        public CatBll(CatDal dal)
        {
            _cats = dal;
            _medical = new MedicalDal();
            _adoptions = new AdoptionDal();
        }

        private CatBll(DbHelper db)
        {
            _cats = new CatDal(db);
            _medical = new MedicalDal(db);
            _adoptions = new AdoptionDal(db);
        }

        public HomeCounts GetHomeCounts()
        {
            return _cats.GetHomeCounts();
        }

        public List<CatListItem> GetRecent()
        {
            return _cats.GetRecent();
        }

        public CatDetail GetDetail(int catId)
        {
            return _cats.GetDetail(catId);
        }

        public RescueRecord GetRescue(int catId)
        {
            return _cats.GetRescue(catId);
        }

        public PagedResult<CatListItem> Search(CatSearch query)
        {
            if (query == null)
            {
                query = new CatSearch();
            }
            int pageIndex = query.PageIndex < 1 ? 1 : query.PageIndex;
            string keyword = FieldRules.Clean(query.Keyword);
            PagedResult<CatListItem> page = _cats.Search(keyword, query.Status, query.Gender, query.RescueFrom, query.RescueTo, pageIndex, PageSize);
            int pages = page.TotalCount == 0 ? 1 : (page.TotalCount + PageSize - 1) / PageSize;
            if (pageIndex > pages)
            {
                pageIndex = pages;
                page = _cats.Search(keyword, query.Status, query.Gender, query.RescueFrom, query.RescueTo, pageIndex, PageSize);
            }
            page.PageIndex = pageIndex;
            page.PageSize = PageSize;
            return page;
        }

        public OperationResult<int> Create(CatArchive cat, RescueRecord rescue, int userId)
        {
            TrimArchive(cat);
            TrimRescue(rescue);
            cat.CreateUserId = userId;
            rescue.ReceiverUserId = userId;
            string error = FieldRules.First(ValidateArchive(cat), ValidateRescue(rescue));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _cats.Create(cat, rescue);
        }

        public OperationResult<int> Update(CatArchive cat)
        {
            TrimArchive(cat);
            CatDetail detail = _cats.GetDetail(cat.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanEditArchive(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            string error = ValidateArchive(cat);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _cats.Update(cat);
        }

        public OperationResult<int> UpdateRescue(RescueRecord rescue)
        {
            TrimRescue(rescue);
            CatDetail detail = _cats.GetDetail(rescue.CatId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanEditArchive(detail.Cat.Status))
            {
                return OperationResult<int>.Fail(Messages.StatusNotAllowed);
            }
            string error = FieldRules.First(ValidateRescue(rescue), RescueAgainstChildren(rescue));
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _cats.UpdateRescue(rescue);
        }

        public OperationResult<int> Delete(int catId)
        {
            CatDetail detail = _cats.GetDetail(catId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            if (!CatRules.CanDeleteCat(detail.MedicalCount, detail.AdoptionCount))
            {
                return OperationResult<int>.Fail(Messages.CatHasChildren);
            }
            return _cats.Delete(catId);
        }

        public string CloseBlock(int catId)
        {
            CatDetail detail = _cats.GetDetail(catId);
            if (detail == null)
            {
                return Messages.NotFound;
            }
            return Evaluate(detail, CatStatuses.Closed, null);
        }

        public OperationResult<int> ChangeStatus(int catId, byte target, string reason)
        {
            CatDetail detail = _cats.GetDetail(catId);
            if (detail == null)
            {
                return OperationResult<int>.Fail(Messages.NotFound);
            }
            string error = Evaluate(detail, target, reason);
            if (error != null)
            {
                return OperationResult<int>.Fail(error);
            }
            return _cats.ChangeStatus(catId, target, FieldRules.Clean(reason));
        }

        private string Evaluate(CatDetail detail, byte target, string reason)
        {
            int follows = target == CatStatuses.Closed ? ActiveFollowCount(detail.Cat.CatId) : 0;
            return CatRules.ChangeStatus(detail.Cat.Status, target, follows, reason);
        }

        private int ActiveFollowCount(int catId)
        {
            List<AdoptionRecord> rows = _adoptions.ListByCat(catId);
            for (int i = 0; i < rows.Count; i++)
            {
                if (rows[i].Status == AdoptionStatuses.Active)
                {
                    return rows[i].FollowUpCount;
                }
            }
            return 0;
        }

        private string RescueAgainstChildren(RescueRecord rescue)
        {
            List<MedicalRecord> medicals = _medical.ListByCat(rescue.CatId);
            for (int i = 0; i < medicals.Count; i++)
            {
                if (medicals[i].MedicalDate.Date < rescue.RescueTime.Date)
                {
                    return Messages.MedicalBeforeRescue;
                }
            }
            List<AdoptionRecord> adoptions = _adoptions.ListByCat(rescue.CatId);
            for (int i = 0; i < adoptions.Count; i++)
            {
                if (adoptions[i].AdoptDate.Date < rescue.RescueTime.Date)
                {
                    return Messages.AdoptBeforeRescue;
                }
            }
            return null;
        }

        private static void TrimArchive(CatArchive cat)
        {
            cat.TempName = FieldRules.Clean(cat.TempName);
            cat.EstimatedAge = FieldRules.Clean(cat.EstimatedAge);
            cat.Color = FieldRules.Clean(cat.Color);
            cat.Remark = FieldRules.Clean(cat.Remark);
            cat.PhotoPath = FieldRules.Clean(cat.PhotoPath);
        }

        private static void TrimRescue(RescueRecord rescue)
        {
            rescue.Location = FieldRules.Clean(rescue.Location);
            rescue.FoundWay = FieldRules.Clean(rescue.FoundWay);
            rescue.InitialHealth = FieldRules.Clean(rescue.InitialHealth);
            rescue.Remark = FieldRules.Clean(rescue.Remark);
        }

        private static string ValidateArchive(CatArchive cat)
        {
            return FieldRules.First(
                FieldRules.Required(cat.TempName, 50),
                FieldRules.Optional(cat.EstimatedAge, 20),
                FieldRules.Optional(cat.Color, 50),
                FieldRules.Optional(cat.Remark, 500),
                FieldRules.Optional(cat.PhotoPath, 200),
                ByteCode(cat.Gender, 2),
                ByteCode(cat.IsNeutered, 2));
        }

        private static string ValidateRescue(RescueRecord rescue)
        {
            return FieldRules.First(
                FieldRules.RealDate(rescue.RescueTime),
                FieldRules.NotAfterToday(rescue.RescueTime),
                FieldRules.Required(rescue.Location, 200),
                FieldRules.Required(rescue.InitialHealth, 500),
                FieldRules.Optional(rescue.Remark, 500),
                FieldRules.Choice(rescue.FoundWay, Lookups.FoundWays, Messages.BadFoundWay));
        }

        private static string ByteCode(byte value, byte max)
        {
            return value <= max ? null : Messages.RequiredField;
        }
    }
}
