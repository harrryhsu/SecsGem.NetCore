namespace SecsGem.NetCore.Enum
{
    public static class SECS_RESPONSE
    {
        public enum PPGNT
        {
            Ok = 0,

            AlreadyHave,

            NoSpace,

            InvalidPPID,

            Busy,

            WillNotAccept,

            OtherError
        }

        public enum ACKC7
        {
            Accept = 0,

            PermissionNotGranted,

            LengthError,

            MatrixOverflow,

            PPIDNotFound,

            UnSupportedMode,

            AsyncCompletion,

            StorageLimitError
        }

        public enum EAC
        {
            Ok = 0,

            OneOrMoreConstantDoNotExist,

            Busy,

            OneOrMoreValueOutOfRange
        }

        public enum DRACK
        {
            Ok = 0,

            OutOfSpace,

            InvalidFormat,

            AlreadyDefined,

            InvalidVid,
        }

        public enum LRACK
        {
            Ok = 0,

            OutOfSpace,

            InvalidFormat,

            OneOrMoreCeidAlreadyDefined,

            OneOrMoreCeidInvalid,

            OneOrMoreRptidInvalid,
        }

        public enum HCACK
        {
            Ok = 0,

            InvalidCommand,

            CannotDoNow,

            ParameterError,

            AsyncCompletion,

            Rejected,

            InvalidObject
        }
    }
}