using SharpDX.DirectInput;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace kadmium_osc_dmx_firesafety
{
    [SettingsSerializeAs(SettingsSerializeAs.Xml)]
    class InputDeviceWrapper : IEquatable<InputDeviceWrapper>
    {
        public DeviceInstance Device { get; set; }
        public string Name
        {
            get
            {
                return Device.InstanceName;
            }
        }

        public InputDeviceWrapper(DeviceInstance instance)
        {
            Device = instance;
        }

        public bool Equals(InputDeviceWrapper other)
        {
            if(Device == null && other.Device == null)
            {
                return true;
            }
            if((Device == null && other.Device != null) || (Device != null && other.Device == null))
            {
                return false;
            }
            return Device.InstanceGuid == other.Device.InstanceGuid;
        }
    }
}
