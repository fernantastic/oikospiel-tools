namespace AudioSynthesis.Midi.Event
{
    using System;

    public class MidiEvent
    {
        protected int absoluteTime;
        protected int time;
        protected int message;

        public int AbsoluteTime
        {
            get { return absoluteTime; }
            set { absoluteTime = value; }
        }
        public int DeltaTime
        {
            get { return time; }
            set { time = value; }
        }
        public virtual int Channel
        {
            get { return message & 0x000000F; }
        }
        public virtual int Command
        {
            get { return (message & 0x00000F0); }
        }
        public int Data1
        {
            get { return (message & 0x000FF00) >> 8; }
        }
        public int Data2
        {
            get { return (message & 0x0FF0000) >> 16; }
        }

        public MidiEvent(int delta, byte status, byte data1, byte data2, int absolutetime = 0)
        {
            this.time = delta;
            this.absoluteTime = absolutetime;
            this.message = status | data1 << 8 | data2 << 16;
        }
        public override string ToString()
        {
            string value = "MidiEvent: " + Enum.GetName(typeof(MidiEventTypeEnum), Command);
            if (Command == 0xB0)
                value += "(" + Enum.GetName(typeof(ControllerTypeEnum), Data1) + ")";
            value += string.Format("time: {0}, channel: {1}, command: {2}, data1: {3}, data2: {4}", time, Channel, Command, Data1, Data2);
            return value;
        }
    }
}
