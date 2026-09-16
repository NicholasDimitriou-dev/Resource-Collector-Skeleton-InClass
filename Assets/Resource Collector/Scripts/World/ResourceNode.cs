using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

/*
 * ResourceNode is a harvestable object like a tree or stone. Replicated health
 * counts down as players hit it with the right tool; at zero the server spawns
 * resource pickups and every client hides the depleted node.
 */

public class ResourceNode : Interactable
{
    [SerializeField] List<ObjectType> _toolTypeRequired = new();
    [SerializeField] NetworkObject _producedPrefab;
    [SerializeField] int _amountToSpawn = 3;
    [SerializeField] int _startingHealth = 1;
    [SerializeField] AudioClip _audioClip;

    readonly NetworkVariable<int> _health = new();

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // TODO Slice 8.1: on the server, set health to _startingHealth. Then
        // subscribe to health changes and apply the current health.
        if (IsServer)
        {
            _health.Value = _startingHealth;
        }
        _health.OnValueChanged += HandleHealthChanged;
        HandleHealthChanged(_health.Value, _health.Value);
    }

    public override void OnNetworkDespawn()
    {
        // TODO Slice 8.4: unsubscribe from replicated health changes.
        base.OnNetworkDespawn();
        _health.OnValueChanged -= HandleHealthChanged;
    }

    // private void LateUpdate()
    // {
    //     HandleHealthChanged(_health.Value, _health.Value);
    // }

    public override bool CanInteract(ObjectType heldType)
    {
        // TODO Slice 8.5: require a living node and an accepted tool.
        return (_health.Value != 0 && _toolTypeRequired.Contains(heldType));
    }

    protected override void Interact(PlayerHeldItem heldItem)
    {
        // TODO Slice 8.7: reduce health and play feedback. At zero, spawn
        // _amountToSpawn copies of _producedPrefab with InstantiateAndSpawn.
        // Place each on the ground with a small random XZ offset and random yaw.
        // </> end of Slice 8
        HitFeedbackRpc();
        HandleHealthChanged(_health.Value, _health.Value-1);
        //_health.Value--;
        if (_health.Value == 0)
        {
            for(int i = 0; i< _amountToSpawn; i++)
            {
                float x = Random.Range(0, 5);
                float z =  Random.Range(0, 5);
                float yaw = Random.Range(0, 360);
                Vector3 idk = new Vector3(transform.position.x + x, transform.position.y, transform.position.z);
                //Quaternion rot = new Quaternion(transform.rotation.x, yaw, transform.rotation.z);
                NetworkObject.InstantiateAndSpawn(_producedPrefab.gameObject,NetworkManager, position:idk,rotation:Quaternion.identity);
            }
            
        }
        
    }

    [Rpc(SendTo.ClientsAndHost)]
    void HitFeedbackRpc()
    {
        // TODO Slice 8.6: play the authored hit sound on each observer.
        AudioSource.PlayClipAtPoint(_audioClip,transform.position);
    }

    void HandleHealthChanged(int previousValue, int newValue)
    {
        // TODO Slice 8.3: apply replicated health locally.
        if (previousValue >= newValue && _health.Value >= previousValue)
        {
            
            _health.Value = newValue;
            ApplyHealth();
        }
    }

    void ApplyHealth()
    {
        // TODO Slice 8.2: hide depleted nodes and disable their collider.
        if (_health.Value == 0)
        {
            gameObject.GetComponent<Renderer>().enabled = false;
            gameObject.GetComponent<Collider>().enabled = false;
        }

    }
}
