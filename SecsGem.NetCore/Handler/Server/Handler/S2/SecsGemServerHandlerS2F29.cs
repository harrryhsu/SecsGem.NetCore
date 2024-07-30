using SecsGem.NetCore.Feature.Server;
using SecsGem.NetCore.Handler.Common;
using SecsGem.NetCore.Hsms;

namespace SecsGem.NetCore.Handler.Server
{
    [SecsGemStream(2, 29)]
    [SecsGemFunctionType(SecsGemFunctionType.ReadOnly)]
    public class SecsGemServerHandlerS2F29 : SecsGemServerStreamHandler
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
                    ?? new EquipmentConstant { Id = x.GetU4() }
                ).ToList();
            }

            await Context.ReplyAsync(
                HsmsMessage.Builder
                    .Reply(Context.Message)
                    .Item(new ListDataItem(
                        items.Select(x => new ListDataItem(
                            new U4DataItem(x.Id),
                            new ADataItem(x.Name),
                            new ADataItem(x.Min.ToString()),
                            new ADataItem(x.Max.ToString()),
                            new ADataItem(x.Default.ToString()),
                            new ADataItem(x.Unit)
                        )).ToArray()
                    ))
                    .Build()
            );
        }
    }
}