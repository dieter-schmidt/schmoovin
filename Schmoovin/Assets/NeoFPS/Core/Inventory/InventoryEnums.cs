
namespace NeoFPS
{
    public enum DuplicateEntryBehaviour
    {
        Reject,
        DestroyOld,
        DropOld
    }

    public enum StartingSlot
    {
        Ascending,
        Descending,
        CustomOrder
    }

    public enum WieldableDeselectAction
    {
        DeactivateGameObject,
        DisableWieldableComponent,
        Nothing
    }

    public enum SwapAction
    {
        Drop,
        Destroy
    }
}
