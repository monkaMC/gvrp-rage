using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using Vector3 = GTANetworkAPI.Vector3;

namespace GVRP.Module.Delivery
{
    public class DeliveryOrder 
    {
        public Dictionary<Vector3, bool> DeliveryPositions { get; set; }
        public DeliveryJob DeliveryJob { get; set; }
        public DeliveryJobType DeliveryJobType { get; set; }
        public DateTime DeliveryStart { get; set; }

        public Vector3 NextPosition { get; set; }

        public DeliveryOrder(Dictionary<Vector3, bool> deliveryPositions, DeliveryJob deliveryJob, DeliveryJobType deliveryJobType)
        {
            this.DeliveryPositions = deliveryPositions;
            this.DeliveryJob = deliveryJob;
            this.DeliveryJobType = deliveryJobType;
            this.DeliveryStart = DateTime.Now;
            this.NextPosition = null;
        }
    }
}
