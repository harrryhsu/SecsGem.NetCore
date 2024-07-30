using SecsGem.NetCore.Event.Client;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(6, 11)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemClientHandlerS6F11 : SecsGemClientStreamHandler
    {
        public override async Task Execute()
        {
            var dataId = Context.Message.Root[0].GetU4();
            var ceid = Context.Message.Root[1].GetU4();

            var ce = new CollectionEvent
            {
                Id = ceid,
                CollectionReports = Context.Message.Root[2]
                    .GetListItem()
                    .Select(report => new CollectionReport
                    {
                        Id = report[0].GetU4(),
                        DataVariables = report[1]
                            .GetListItem()
                            .Select(dv => new DataVariable
                            {
                                Value = dv.GetString(),
                            })
                            .ToList()
                    })
                    .ToList()
            };

            await Context.Kernel.Emit(new SecsGemCollectionEventEvent
            {
                CollectionEvent = ce,
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem(0))
                    .Build()
            );
        }
    }
}