using SecsGem.NetCore.State.Server;

namespace SecsGem.NetCore.Handler.Common
{
    public enum SecsGemFunctionType
    {
        Communication = GemServerStateModel.Selected,

        ReadOnly = GemServerStateModel.ControlOnlineLocal,

        Operation = GemServerStateModel.ControlOnlineRemote
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class SecsGemFunctionTypeAttribute : Attribute
    {
        public SecsGemFunctionType Type { get; set; }

        public SecsGemFunctionTypeAttribute(SecsGemFunctionType type)
        {
            Type = type;
        }
    }
}