using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public abstract class NetworkObject : MonoBehaviour
{
    public abstract void SendNetworkData();
    public abstract void ReceiveNetworkData();
}