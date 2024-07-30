using SecsGem.NetCore.Event.Server;
using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 13)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS2F13 : SecsGemServerStreamHandler
    {
        public override async Task Execute()
        {
            IEnumerable<EquipmentConstant> items;
            if (Context.Message.Root.Count == 0)
            {
                items = Context.Kernel.Feature.EquipmentConstants;
            }
            else
            {
                items = Context.Message.Root.GetListItem().Select(x =>
                    Context.Kernel.Feature.EquipmentConstants.FirstOrDefault(y => y.Id == x.GetU4())
                ).ToList();
            }

            await Context.Kernel.Emit(new SecsGemGetEquipmentConstantEvent
            {
                Params = items
            });

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        items.Select<EquipmentConstant, DataItem>(x => x == null ? new ListDataItem() : new ADataItem(x.Value.ToString())).ToArray()
                    ))
                    .Build()
            );
        }
    }
}