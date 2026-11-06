namespace StrayCat.Model
{
    public static class Messages
    {
        public const string BadLogin = "用户名或密码错误";
        public const string ChooseLogin = "请选择用户登录或管理员登录";
        public const string UseAdminLogin = "请使用管理员登录";
        public const string UseUserLogin = "请使用用户登录";
        public const string Disabled = "账号已停用";
        public const string UserNameExists = "用户名已存在";
        public const string Forbidden = "没有权限访问该页面";
        public const string Expired = "登录已过期，请重新登录";
        public const string StatusNotAllowed = "当前状态不允许此操作";
        public const string TerminateReasonRequired = "请填写终止原因";
        public const string FollowUpRequired = "至少有一条回访才能结案";
        public const string ActiveAdoptionExists = "该猫已有一条有效领养";
        public const string CatHasChildren = "存在医疗或领养记录，不能删除该档案";
        public const string AdoptionHasFollowUp = "存在回访记录，不能删除该领养";
        public const string CannotDisableSelf = "不能停用当前登录账号";
        public const string CannotDisableLastAdmin = "不能停用或降级最后一个管理员";
        public const string PhotoType = "只允许上传 jpg 或 png 图片";
        public const string PhotoSize = "图片不能超过 2MB";
        public const string NotFound = "记录不存在或已被删除";
        public const string CatNoExhausted = "当年档案编号已用完";
        public const string CatNoConflict = "档案编号冲突，请重新保存";
        public const string RescueExists = "该猫已有接收记录";
        public const string EmptyList = "暂无记录";
        public const string UserNameRule = "用户名须为 4 到 20 位字母或数字";
        public const string PasswordRule = "密码须为 6 到 20 位";
        public const string PhoneRule = "请填写正确的手机号";
        public const string RequiredField = "请填写必填项";
        public const string DateAfterToday = "日期不能晚于今天";
        public const string MedicalBeforeRescue = "医疗日期不能早于接收日期";
        public const string AdoptBeforeRescue = "领养日期不能早于接收日期";
        public const string ReturnBeforeAdopt = "退养日期不能早于领养日期";
        public const string FollowBeforeAdopt = "回访日期不能早于领养日期";
        public const string BadFoundWay = "请选择发现方式";
        public const string BadFollowWay = "请选择回访方式";
        public const string BadDate = "请填写正确的日期";
        public const string DateOrder = "开始日期不能晚于结束日期";
        public const string PasswordMismatch = "两次输入的密码不一致";
    }
}
