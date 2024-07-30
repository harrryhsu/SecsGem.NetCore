using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 35)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS2F35 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var dataId = Context.Message.Root[0].GetU4();
            var events = Context.Message.Root[1].GetListItem();

            if (events.Count == 0)
            {
                Context.Kernel.Feature.CollectionEvents.ForEach(x => x.CollectionReports.Clear());
                await Context.ReplyAsync(
                   HsmsMessage.Builder
                       .Reply(Context.Message)
                       .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.Ok))
                       .Build()
                );
                return;
            }

            foreach (var evt in events)
            {
                var id = evt[0].GetU4();
                var rpids = evt[1].GetListItem().Select(x => x.GetU4());
                var existing = Context.Kernel.Feature.CollectionEvents.FirstOrDefault(x => x.Id == id);

                if (existing == null)
                {
                    await Context.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(Context.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.OneOrMoreCeidInvalid))
                            .Build()
                    );
                    return;
                }
                else if (rpids.Any(x => Context.Kernel.Feature.CollectionReports.All(y => y.Id != x)))
                {
                    await Context.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(Context.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.OneOrMoreRptidInvalid))
                            .Build()
                    );
                    return;
                }
                else
                {
                    foreach (var rpid in rpids)
                    {
                        var existingReport = existing.CollectionReports.FirstOrDefault(x => x.Id == rpid);
                        if (existingReport != null)
                        {
                            await Context.ReplyAsync(
                                HsmsMessage.Builder
                                    .Reply(Context.Message)
                                    .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.OneOrMoreCeidAlreadyDefined))
                                    .Build()
                            );
                            return;
                        }
                    }
                }
            }

            foreach (var evt in events)
            {
                var id = evt[0].GetU4();
                var rpids = evt[1].GetListItem().Select(x => x.GetU4());
                var existing = Context.Kernel.Feature.CollectionEvents.FirstOrDefault(x => x.Id == id);

                if (rpids.Count() == 0)
                {
                    existing.CollectionReports.Clear();
                }
                else
                {
                    foreach (var rpid in rpids)
                    {
                        var existingReport = Context.Kernel.Feature.CollectionReports.First(x => x.Id == rpid);
                        existing.CollectionReports.Add(existingReport);
                    }
                }
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)SECS_RESPONSE.LRACK.Ok))
                    .Build()
            );
        }
    }
}