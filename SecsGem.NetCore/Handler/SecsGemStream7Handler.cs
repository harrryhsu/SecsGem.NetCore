using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler
{
    public class SecsGemStream7Handler : ISecsGemStreamHandler
    {
        public async Task S7F1(SecsGemRequestContext req)
        {
            var id = req.Message.Root[0].GetString();
            var pp = req.Kernel.Feature.ProcessPrograms.FirstOrDefault(x => x.Id == id);
            if (pp == null)
            {
                req.Kernel.Feature.ProcessPrograms.Add(new Feature.ProcessProgram { Id = id });
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)(pp != null ? 0x0 : 0x1)))
                    .Build()
            );
        }

        public async Task S7F3(SecsGemRequestContext req)
        {
            var id = req.Message.Root[0].GetString();
            var body = req.Message.Root[1].GetBins();
            var pp = req.Kernel.Feature.ProcessPrograms.FirstOrDefault(x => x.Id == id);

            if (pp != null)
            {
                pp.Body = body;
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)(pp != null ? 0x0 : 0x4)))
                    .Build()
            );
        }

        public async Task S7F5(SecsGemRequestContext req)
        {
            var id = req.Message.Root[0].GetString();
            var pp = req.Kernel.Feature.ProcessPrograms.FirstOrDefault(x => x.Id == id) ?? new Feature.ProcessProgram { Id = id };

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        new ADataItem(id),
                        new BinDataItem(pp.Body)
                    ))
                    .Build()
            );
        }

        public async Task S7F17(SecsGemRequestContext req)
        {
            var id = req.Message.Root[0].GetString();
            var pp = req.Kernel.Feature.ProcessPrograms.FirstOrDefault(x => x.Id == id);

            if (pp != null)
            {
                req.Kernel.Feature.ProcessPrograms.Remove(pp);
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)(pp != null ? 0x0 : 0x4)))
                    .Build()
            );
        }

        public async Task S7F19(SecsGemRequestContext req)
        {
            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new ListDataItem(
                        req.Kernel.Feature.ProcessPrograms.Select(x => new ADataItem(x.Id)).ToArray()
                    ))
                    .Build()
            );
        }
    }
}