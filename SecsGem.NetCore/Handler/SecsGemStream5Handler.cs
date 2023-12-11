using SecsGem.NetCore.Feature;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler
{
    public class SecsGemStream5Handler : ISecsGemStreamHandler
    {
        public async Task S5F3(SecsGemRequestContext req)
        {
            var enabled = req.Message.Root[0].GetBin() == 128;
            var id = req.Message.Root[1].GetU4();
            var alarm = req.Kernel.Feature.Alarms.FirstOrDefault(x => x.Id == id);

            if (alarm != null)
            {
                alarm.Enabled = enabled;
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem(0x0))
                    .Build()
            );
        }

        public async Task S5F5(SecsGemRequestContext req)
        {
            var ids = req.Message.Root.Cast<U4DataItem>();
            IEnumerable<Alarm> items;
            if (ids.Count == 0)
            {
                items = req.Kernel.Feature.Alarms;
            }
            else
            {
                items = ids.Values.Select(x => req.Kernel.Feature.Alarms.FirstOrDefault(y => y.Id == x) ?? new Alarm { Id = x }).ToList();
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
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

        public async Task S5F7(SecsGemRequestContext req)
        {
            var alarms = req.Kernel.Feature.Alarms.Where(x => x.Enabled).ToList();

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
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