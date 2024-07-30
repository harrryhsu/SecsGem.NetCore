using SecsGem.NetCore.Enum;
using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 15)]
    [SecsGemFunctionType(SecsGemFunctionType.Operation)]
    public class SecsGemServerHandlerS2F15 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            List<EquipmentConstant> ecs = new();
            foreach (var item in Context.Message.Root.GetListItem())
            {
                var id = item[0].GetU4();
                if (!int.TryParse(item[1].GetString(), out var intVal))
                {
                    await Context.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(Context.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.EAC.OneOrMoreValueOutOfRange))
                            .Build()
                    );
                    return;
                }

                var ec = Context.Kernel.Feature.EquipmentConstants.FirstOrDefault(x => x.Id == id);
                if (ec == null)
                {
                    await Context.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(Context.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.EAC.OneOrMoreConstantDoNotExist))
                            .Build()
                    );
                    return;
                }

                if (intVal > ec.Max || intVal < ec.Min)
                {
                    await Context.ReplyAsync(
                        HsmsMessage.Builder
                            .Reply(Context.Message)
                            .Item(new BinDataItem((byte)SECS_RESPONSE.EAC.OneOrMoreValueOutOfRange))
                            .Build()
                    );
                    return;
                }

                ec.Value = intVal;
                ecs.Add(ec);
            }

            await Context.Kernel.Emit(new SecsGemSetEquipmentConstantEvent
            {
                Params = ecs
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new BinDataItem((byte)SECS_RESPONSE.EAC.Ok))
                    .Build()
            );
        }
    }
}