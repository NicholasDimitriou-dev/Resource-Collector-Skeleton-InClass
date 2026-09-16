using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/*
 * Receptacle collects one resource type, like a pallet accepting wood. A
 * replicated stack count drives which stacked visuals are shown, so current
 * and late-joining clients render the same pile.
 */

public class Receptacle : Interactable
{
    [SerializeField] ObjectType _acceptedObjectType;
    [SerializeField] List<GameObject> _stackedResourceVisuals = new();
    [SerializeField] AudioClip _audioClip;

    readonly NetworkVariable<int> _stackedCount = new();

    public bool IsFilled => _stackedCount.Value >= _stackedResourceVisuals.Count;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // TODO Slice 9.1: initialize on the server, subscribe, and apply current count.
        if (IsServer)
        {
            _stackedCount.Value = 0;
            
        }
        _stackedCount.OnValueChanged += HandleStackedCountChanged;
        HandleStackedCountChanged(_stackedCount.Value,_stackedCount.Value);
       
    }

    public override void OnNetworkDespawn()
    {
        // TODO Slice 9.4: unsubscribe from replicated count changes.
        base.OnNetworkDespawn();
        _stackedCount.OnValueChanged -= HandleStackedCountChanged;
    }

    public override bool CanInteract(ObjectType heldType)
    {
        // TODO Slice 9.5: accept only the configured resource while space remains.
        return heldType.Equals(_acceptedObjectType);
    }

    protected override void Interact(PlayerHeldItem heldItem)
    {
        if (!CanInteract(heldItem.ObjectType)) return;
        // TODO Slice 9.6: add one resource and clear the player's hand. </> end of Slice 9
        heldItem.Clear();
        //_stackedCount.Value++;
    }

    void HandleStackedCountChanged(int previousValue, int newValue)
    {
        // TODO Slice 9.3: always apply newValue to the visuals. Play audio only
        // when the stack grows
        if (previousValue >= newValue && _stackedCount.Value >= previousValue)
        {
            _stackedCount.Value = newValue;
            ApplyStackedCount(_stackedCount.Value);
        }
    }

    void ApplyStackedCount(int count)
    {
        // TODO Slice 9.2: show exactly the first count visuals.
        for (int i = 0; i < count; i++)
        {
          _stackedResourceVisuals[i].gameObject.SetActive(true);   
        }
    }
}
