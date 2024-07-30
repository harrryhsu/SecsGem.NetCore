using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(5, 5)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS5F5 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var ids = Context.Message.Root.Cast<U4DataItem>();
            IEnumerable<Alarm> items;
            if (ids.Count == 0)
            {
                items = Context.Kernel.Feature.Alarms;
            }
            else
            {
                items = ids.Values.Select(x => Context.Kernel.Feature.Alarms.FirstOrDefault(y => y.Id == x) ?? new Alarm { Id = x }).ToList();
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ListDataItem(
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