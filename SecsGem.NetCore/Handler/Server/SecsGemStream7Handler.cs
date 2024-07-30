using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    public class SecsGemStream7Handler : ISecsGemServerStreamHandler
    {
        public async Task S7F1(SecsGemServerRequestContext req)
        {
            var id = req.Message.Root[0].GetString();
            var pp = req.Kernel.Feature.ProcessPrograms.FirstOrDefault(x => x.Id == id);
            if (pp == null)
            {
                req.Kernel.Feature.ProcessPrograms.Add(new ProcessProgram { Id = id });
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)(pp != null ? SECS_RESPONSE.PPGNT.AlreadyHave : SECS_RESPONSE.PPGNT.Ok)))
                    .Build()
            );
        }

        public async Task S7F3(SecsGemServerRequestContext req)
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
                    .Item(new BinDataItem((byte)(pp != null ? SECS_RESPONSE.ACKC7.Accept : SECS_RESPONSE.ACKC7.PPIDNotFound)))
                    .Build()
            );
        }

        public async Task S7F5(SecsGemServerRequestContext req)
        {
            var id = req.Message.Root.GetString();
            var pp = req.Kernel.Feature.ProcessPrograms.FirstOrDefault(x => x.Id == id) ?? new ProcessProgram { Id = id };

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

        public async Task S7F17(SecsGemServerRequestContext req)
        {
            if (req.Message.Root.Count == 0)
            {
                req.Kernel.Feature.ProcessPrograms.Clear();
            }
            else
            {
                var pps = req.Message.Root.GetListItem().Select(x => req.Kernel.Feature.ProcessPrograms.FirstOrDefault(y => y.Id == x.GetString()));

                if (pps.Any(x => x == null))
                {
                    await req.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(req.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.ACKC7.PPIDNotFound))
                            .Build()
                    );
                    return;
                }

                foreach (var pp in pps)
                {
                    req.Kernel.Feature.ProcessPrograms.Remove(pp);
                }
            }

            await req.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(req.Message)
                    .Item(new BinDataItem((byte)SECS_RESPONSE.ACKC7.Accept))
                    .Build()
            );
        }

        public async Task S7F19(SecsGemServerRequestContext req)
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