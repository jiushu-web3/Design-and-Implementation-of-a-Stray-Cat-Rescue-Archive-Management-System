using StrayCat.Model;

namespace StrayCat.BLL
{
    public static class CatRules
    {
        public static bool CanEditArchive(byte status)
        {
            return !CatStatuses.IsClosed(status);
        }

        public static bool CanEditMedical(byte status)
        {
            return !CatStatuses.IsClosed(status);
        }

        public static bool CanAddAdoption(byte status)
        {
            return status == CatStatuses.WaitingAdoption;
        }

        public static string AdoptionEligibility(byte status)
        {
            if (status == CatStatuses.WaitingAdoption)
            {
                return "可以登记领养";
            }
            if (status == CatStatuses.InCare)
            {
                return "当前为在站照料，需先通过健康评估改为待领养";
            }
            if (status == CatStatuses.Adopted)
            {
                return "已有有效领养，不能重复登记";
            }
            if (CatStatuses.IsClosed(status))
            {
                return "档案已结束，不能登记领养";
            }
            return "当前状态不能登记领养";
        }

        public static bool CanEditActiveAdoption(byte status)
        {
            return status == CatStatuses.Adopted;
        }

        public static bool CanEditFollowUp(byte catStatus, byte adoptionStatus)
        {
            return catStatus == CatStatuses.Adopted && adoptionStatus == AdoptionStatuses.Active;
        }

        public static bool CanDeleteChildren(byte catStatus)
        {
            return !CatStatuses.IsClosed(catStatus);
        }

        public static bool CanDeleteCat(int medicalCount, int adoptionCount)
        {
            return medicalCount == 0 && adoptionCount == 0;
        }

        public static string ChangeStatus(byte current, byte target, int activeFollowUpCount, string reason)
        {
            if (current == CatStatuses.InCare && target == CatStatuses.WaitingAdoption)
            {
                return null;
            }
            if (current == CatStatuses.WaitingAdoption && target == CatStatuses.InCare)
            {
                return null;
            }
            if (current == CatStatuses.Adopted && target == CatStatuses.Closed)
            {
                return activeFollowUpCount > 0 ? null : Messages.FollowUpRequired;
            }
            if ((current == CatStatuses.InCare || current == CatStatuses.WaitingAdoption) && target == CatStatuses.Terminated)
            {
                return string.IsNullOrWhiteSpace(reason) ? Messages.TerminateReasonRequired : null;
            }
            return Messages.StatusNotAllowed;
        }
    }
}
