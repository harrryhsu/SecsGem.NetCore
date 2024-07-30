using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 33)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS2F33 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            var dataId = Context.Message.Root[0].GetU4();
            var reports = Context.Message.Root[1].GetListItem();

            if (reports.Count == 0)
            {
                Context.Kernel.Feature.CollectionReports.Clear();
                Context.Kernel.Feature.CollectionEvents.ForEach(x => x.CollectionReports.Clear());

                await Context.ReplyAsync(
                    HsmsMessage.Builder
                        .Reply(Context.Message)
                        .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.Ok))
                        .Build()
                );
                return;
            }

            List<CollectionReport> remove = new();
            List<CollectionReport> add = new();

            foreach (var report in reports)
            {
                var id = report[0].GetU4();
                var dvIds = report[1].GetListItem().Select(x => x.GetString());
                var existing = Context.Kernel.Feature.CollectionReports.FirstOrDefault(x => x.Id == id);

                if (dvIds.Count() == 0)
                {
                    if (existing == null)
                    {
                        await Context.ReplyAsync(
                           HsmsMessage.Builder
                               .Reply(Context.Message)
                               .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.InvalidFormat))
                               .Build()
                        );
                        return;
                    }
                    else
                    {
                        remove.Add(existing);
                    }
                }
                else
                {
                    if (existing != null)
                    {
                        await Context.ReplyAsync(
                           HsmsMessage.Builder
                               .Reply(Context.Message)
                               .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.AlreadyDefined))
                               .Build()
                        );
                        return;
                    }
                    else if (dvIds.Any(x => Context.Kernel.Feature.DataVariables.All(y => y.Id != x)))
                    {
                        await Context.ReplyAsync(
                          HsmsMessage.Builder
                              .Reply(Context.Message)
                              .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.InvalidVid))
                              .Build()
                        );
                        return;
                    }
                    else
                    {
                        add.Add(new CollectionReport
                        {
                            Id = id,
                            DataVariables = dvIds.Select(x => Context.Kernel.Feature.DataVariables.First(y => y.Id == x)).ToList()
                        });
                    }
                }
            }

            remove.ForEach(x =>
            {
                Context.Kernel.Feature.CollectionReports.Remove(x);
            });

            add.ForEach(x =>
            {
                Context.Kernel.Feature.CollectionReports.Add(x);
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)SECS_RESPONSE.DRACK.Ok))
                    .Build()
            );
        }
    }
}