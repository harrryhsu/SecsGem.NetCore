using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(5, 7)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS5F7 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var alarms = Context.Kernel.Feature.Alarms.Where(x => x.Enabled).ToList();

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        alarms.Select(x => new ListDataItem(
                            new BinDataItem((byte)(x.Enabled ? 128 : 0)),
                            new U4DataItem(x.Id),
                            new ADataItem(x.Description)
                        )).ToArray()
                    ))
                    .Build()
            );
        }
    }
}