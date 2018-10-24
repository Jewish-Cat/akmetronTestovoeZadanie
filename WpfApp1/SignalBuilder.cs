using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Input;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace WpfApp1
{
    


    abstract class Generator
    {
        public Signal Signal { get; private set; }
        public void GenerateSignal()
        {
            Signal = new Signal();
        }
        public abstract void SetType();
        public abstract void SetParam1(string par1);
        public abstract void SetParam2(string par2);
        public abstract void SetParam3(string par3);
        public abstract void SetDuration(string par4);
    }

    class SignalBuilder
    {
        public Signal Transmit(Generator generator, string par1, string par2, string par3,string par4)
        {
            generator.GenerateSignal();
            generator.SetType();
            generator.SetParam1(par1);
            generator.SetParam2(par2);
            generator.SetParam3(par3);
            generator.SetDuration(par4);
            return generator.Signal;
        }
    }

    class FMGenerator : Generator
    {

        public override void SetType()
        {
            Signal.Type = "FM - сигнал";
        }
        public override void SetParam1(string Frequency)
        {
            Signal.param1 = Frequency;
        }
        public override void SetParam2(string Amplitude)
        {
            Signal.param2 = Amplitude;
        }
        public override void SetParam3(string MSFrequency)
        {
            Signal.param3 = MSFrequency;
        }
        public override void SetDuration(string Duration)
        {
            Signal.duration = Duration;
        }
    }

    class SinusoidalGenerator : Generator
    {
        
        public override void SetType()
        {
            Signal.Type = "Синусоидальный сигнал";
        }
        public override void SetParam1(string Frequency)
        {
            Signal.param1 = Frequency;
        }
        public override void SetParam2(string Amplitude)
        {
            Signal.param2 = Amplitude;
        }
        public override void SetParam3(string PhaseShift)
        {
            Signal.param3 = PhaseShift;
        }
        public override void SetDuration(string Duration)
        {
            Signal.duration = Duration;
        }
    }

    [DataContract]
    class Signal
    {
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public string param1 { get; set; }
        [DataMember]
        public string param2 { get; set; }
        [DataMember]
        public string param3 { get; set; }
        [DataMember]
        public string duration { get; set; }
    }

   
}

