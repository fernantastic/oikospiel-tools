namespace AudioSynthesis.Midi
{
    //structs and enum
    public enum MidiEventTypeEnum
    {
        NoteOff = 0x80,
        NoteOn = 0x90,
        NoteAftertouch = 0xA0,
        Controller = 0xB0,
        ProgramChange = 0xC0,
        ChannelAftertouch = 0xD0,
        PitchBend = 0xE0
    }
    public enum MetaEventTypeEnum
    {
        SequenceNumber = 0x00,
        TextEvent = 0x01,
        CopyrightNotice = 0x02,
        SequenceOrTrackName = 0x03,
        InstrumentName = 0x04,
        LyricText = 0x05,
        MarkerText = 0x06,
        CuePoint = 0x07,
        MidiChannel = 0x20,
        MidiPort = 0x21,
        EndOfTrack = 0x2F,
        Tempo = 0x51,
        SmpteOffset = 0x54,
        TimeSignature = 0x58,
        KeySignature = 0x59,
        SequencerSpecific = 0x7F
    }
    public enum SystemCommonTypeEnum
    {
        SystemExclusive = 0xF0,
        MtcQuarterFrame = 0xF1,
        SongPosition = 0xF2,
        SongSelect = 0xF3,
        TuneRequest = 0xF6
    }
    public enum ControllerTypeEnum
    {
        BankSelectCoarse = 0x00,
        ModulationCoarse = 0x01,
        BreathControllerCoarse = 0x02,
        FootControllerCoarse = 0x04,
        PortamentoTimeCoarse = 0x05,
        DataEntryCoarse = 0x06,
        VolumeCoarse = 0x07,
        BalanceCoarse = 0x08,
        PanCoarse = 0x0A,
        ExpressionControllerCoarse = 0x0B,
        EffectControl1Coarse = 0x0C,
        EffectControl2Coarse = 0x0D,
        GeneralPurposeSlider1 = 0x10,
        GeneralPurposeSlider2 = 0x11,
        GeneralPurposeSlider3 = 0x12,
        GeneralPurposeSlider4 = 0x13,
        BankSelectFine = 0x20,
        ModulationFine = 0x21,
        BreathControllerFine = 0x22,
        FootControllerFine = 0x24,
        PortamentoTimeFine = 0x25,
        DataEntryFine = 0x26,
        VolumeFine = 0x27,
        BalanceFine = 0x28,
        PanFine = 0x2A,
        ExpressionControllerFine = 0x2B,
        EffectControl1Fine = 0x2C,
        EffectControl2Fine = 0x2D,
        HoldPedal = 0x40,
        Portamento = 0x41,
        SostenutoPedal = 0x42,
        SoftPedal = 0x43,
        LegatoPedal = 0x44,
        Hold2Pedal = 0x45,
        SoundVariation = 0x46,
        SoundTimbre = 0x47,
        SoundReleaseTime = 0x48,
        SoundAttackTime = 0x49,
        SoundBrightness = 0x4A,
        SoundControl6 = 0x4B,
        SoundControl7 = 0x4C,
        SoundControl8 = 0x4D,
        SoundControl9 = 0x4E,
        SoundControl10 = 0x4F,
        GeneralPurposeButton1 = 0x50,
        GeneralPurposeButton2 = 0x51,
        GeneralPurposeButton3 = 0x52,
        GeneralPurposeButton4 = 0x53,
        EffectsLevel = 0x5B,
        TremuloLevel = 0x5C,
        ChorusLevel = 0x5D,
        CelesteLevel = 0x5E,
        PhaseLevel = 0x5F,
        DataButtonIncrement = 0x60,
        DataButtonDecrement = 0x61,
        NonRegisteredParameterFine = 0x62,
        NonRegisteredParameterCourse = 0x63,
        RegisteredParameterFine = 0x64,
        RegisteredParameterCourse = 0x65,
        AllSoundOff = 0x78,
        ResetControllers = 0x79,
        LocalKeyboard = 0x7A,
        AllNotesOff = 0x7B,
        OmniModeOff = 0x7C,
        OmniModeOn = 0x7D,
        MonoMode = 0x7E,
        PolyMode = 0x7F
    }

    //static helper methods and constants
    public static class MidiHelper
    {
        //--Constants
        public const int MicroSecondsPerMinute = 60000000; //microseconds in a minute
        public const int MinChannel = 0;
        public const int MaxChannel = 15;
        public const int DrumChannel = 9;
    }

    /// <summary>
    /// Pitches supported by MIDI.
    /// </summary>
    /// <remarks>
    /// <para>MIDI defines 127 distinct pitches, in semitone intervals, ranging from C five octaves
    /// below middle C, up to G five octaves above middle C.  This covers several octaves above and
    /// below the range of a normal 88-key piano.</para>
    /// <para>These 127 pitches are the only ones directly expressible in MIDI. Precise
    /// variations in frequency can be achieved with <see cref="OutputDevice.SendPitchBend">Pitch
    /// Bend</see> messages, though Pitch Bend messages apply to the whole channel at once.</para>
    /// <para>In this enum, pitches are given C Major note names (eg "F", "GSharp") followed
    /// by the octave number.  Octaves use standard piano terminology: Middle C is in
    /// octave 4.  (Note that this is different from "MIDI octaves", which have Middle C in
    /// octave 0.)</para>
    /// <para>This enum has extension methods, such as
    /// <see cref="PitchExtensionMethods.NotePreferringSharps"/> and
    /// <see cref="PitchExtensionMethods.IsInMidiRange"/>, defined in
    /// <see cref="PitchExtensionMethods"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="Note"/>
    /// <seealso cref="Interval"/>
    public enum NotePitch
    {
        /// <summary>C in octave -1.</summary>
        CNeg1 = 0,
        /// <summary>C# in octave -1.</summary>
        CSharpNeg1 = 1,
        /// <summary>D in octave -1.</summary>
        DNeg1 = 2,
        /// <summary>D# in octave -1.</summary>
        DSharpNeg1 = 3,
        /// <summary>E in octave -1.</summary>
        ENeg1 = 4,
        /// <summary>F in octave -1.</summary>
        FNeg1 = 5,
        /// <summary>F# in octave -1.</summary>
        FSharpNeg1 = 6,
        /// <summary>G in octave -1.</summary>
        GNeg1 = 7,
        /// <summary>G# in octave -1.</summary>
        GSharpNeg1 = 8,
        /// <summary>A in octave -1.</summary>
        ANeg1 = 9,
        /// <summary>A# in octave -1.</summary>
        ASharpNeg1 = 10,
        /// <summary>B in octave -1.</summary>
        BNeg1 = 11,

        /// <summary>C in octave 0.</summary>
        C0 = 12,
        /// <summary>C# in octave 0.</summary>
        CSharp0 = 13,
        /// <summary>D in octave 0.</summary>
        D0 = 14,
        /// <summary>D# in octave 0.</summary>
        DSharp0 = 15,
        /// <summary>E in octave 0.</summary>
        E0 = 16,
        /// <summary>F in octave 0.</summary>
        F0 = 17,
        /// <summary>F# in octave 0.</summary>
        FSharp0 = 18,
        /// <summary>G in octave 0.</summary>
        G0 = 19,
        /// <summary>G# in octave 0.</summary>
        GSharp0 = 20,
        /// <summary>A in octave 0.</summary>
        A0 = 21,
        /// <summary>A# in octave 0, usually the lowest key on an 88-key keyboard.</summary>
        ASharp0 = 22,
        /// <summary>B in octave 0.</summary>
        B0 = 23,

        /// <summary>C in octave 1.</summary>
        C1 = 24,
        /// <summary>C# in octave 1.</summary>
        CSharp1 = 25,
        /// <summary>D in octave 1.</summary>
        D1 = 26,
        /// <summary>D# in octave 1.</summary>
        DSharp1 = 27,
        /// <summary>E in octave 1.</summary>
        E1 = 28,
        /// <summary>F in octave 1.</summary>
        F1 = 29,
        /// <summary>F# in octave 1.</summary>
        FSharp1 = 30,
        /// <summary>G in octave 1.</summary>
        G1 = 31,
        /// <summary>G# in octave 1.</summary>
        GSharp1 = 32,
        /// <summary>A in octave 1.</summary>
        A1 = 33,
        /// <summary>A# in octave 1.</summary>
        ASharp1 = 34,
        /// <summary>B in octave 1.</summary>
        B1 = 35,

        /// <summary>C in octave 2.</summary>
        C2 = 36,
        /// <summary>C# in octave 2.</summary>
        CSharp2 = 37,
        /// <summary>D in octave 2.</summary>
        D2 = 38,
        /// <summary>D# in octave 2.</summary>
        DSharp2 = 39,
        /// <summary>E in octave 2.</summary>
        E2 = 40,
        /// <summary>F in octave 2.</summary>
        F2 = 41,
        /// <summary>F# in octave 2.</summary>
        FSharp2 = 42,
        /// <summary>G in octave 2.</summary>
        G2 = 43,
        /// <summary>G# in octave 2.</summary>
        GSharp2 = 44,
        /// <summary>A in octave 2.</summary>
        A2 = 45,
        /// <summary>A# in octave 2.</summary>
        ASharp2 = 46,
        /// <summary>B in octave 2.</summary>
        B2 = 47,

        /// <summary>C in octave 3.</summary>
        C3 = 48,
        /// <summary>C# in octave 3.</summary>
        CSharp3 = 49,
        /// <summary>D in octave 3.</summary>
        D3 = 50,
        /// <summary>D# in octave 3.</summary>
        DSharp3 = 51,
        /// <summary>E in octave 3.</summary>
        E3 = 52,
        /// <summary>F in octave 3.</summary>
        F3 = 53,
        /// <summary>F# in octave 3.</summary>
        FSharp3 = 54,
        /// <summary>G in octave 3.</summary>
        G3 = 55,
        /// <summary>G# in octave 3.</summary>
        GSharp3 = 56,
        /// <summary>A in octave 3.</summary>
        A3 = 57,
        /// <summary>A# in octave 3.</summary>
        ASharp3 = 58,
        /// <summary>B in octave 3.</summary>
        B3 = 59,

        /// <summary>C in octave 4, also known as Middle C.</summary>
        C4 = 60,
        /// <summary>C# in octave 4.</summary>
        CSharp4 = 61,
        /// <summary>D in octave 4.</summary>
        D4 = 62,
        /// <summary>D# in octave 4.</summary>
        DSharp4 = 63,
        /// <summary>E in octave 4.</summary>
        E4 = 64,
        /// <summary>F in octave 4.</summary>
        F4 = 65,
        /// <summary>F# in octave 4.</summary>
        FSharp4 = 66,
        /// <summary>G in octave 4.</summary>
        G4 = 67,
        /// <summary>G# in octave 4.</summary>
        GSharp4 = 68,
        /// <summary>A in octave 4.</summary>
        A4 = 69,
        /// <summary>A# in octave 4.</summary>
        ASharp4 = 70,
        /// <summary>B in octave 4.</summary>
        B4 = 71,

        /// <summary>C in octave 5.</summary>
        C5 = 72,
        /// <summary>C# in octave 5.</summary>
        CSharp5 = 73,
        /// <summary>D in octave 5.</summary>
        D5 = 74,
        /// <summary>D# in octave 5.</summary>
        DSharp5 = 75,
        /// <summary>E in octave 5.</summary>
        E5 = 76,
        /// <summary>F in octave 5.</summary>
        F5 = 77,
        /// <summary>F# in octave 5.</summary>
        FSharp5 = 78,
        /// <summary>G in octave 5.</summary>
        G5 = 79,
        /// <summary>G# in octave 5.</summary>
        GSharp5 = 80,
        /// <summary>A in octave 5.</summary>
        A5 = 81,
        /// <summary>A# in octave 5.</summary>
        ASharp5 = 82,
        /// <summary>B in octave 5.</summary>
        B5 = 83,

        /// <summary>C in octave 6.</summary>
        C6 = 84,
        /// <summary>C# in octave 6.</summary>
        CSharp6 = 85,
        /// <summary>D in octave 6.</summary>
        D6 = 86,
        /// <summary>D# in octave 6.</summary>
        DSharp6 = 87,
        /// <summary>E in octave 6.</summary>
        E6 = 88,
        /// <summary>F in octave 6.</summary>
        F6 = 89,
        /// <summary>F# in octave 6.</summary>
        FSharp6 = 90,
        /// <summary>G in octave 6.</summary>
        G6 = 91,
        /// <summary>G# in octave 6.</summary>
        GSharp6 = 92,
        /// <summary>A in octave 6.</summary>
        A6 = 93,
        /// <summary>A# in octave 6.</summary>
        ASharp6 = 94,
        /// <summary>B in octave 6.</summary>
        B6 = 95,

        /// <summary>C in octave 7.</summary>
        C7 = 96,
        /// <summary>C# in octave 7.</summary>
        CSharp7 = 97,
        /// <summary>D in octave 7.</summary>
        D7 = 98,
        /// <summary>D# in octave 7.</summary>
        DSharp7 = 99,
        /// <summary>E in octave 7.</summary>
        E7 = 100,
        /// <summary>F in octave 7.</summary>
        F7 = 101,
        /// <summary>F# in octave 7.</summary>
        FSharp7 = 102,
        /// <summary>G in octave 7.</summary>
        G7 = 103,
        /// <summary>G# in octave 7.</summary>
        GSharp7 = 104,
        /// <summary>A in octave 7.</summary>
        A7 = 105,
        /// <summary>A# in octave 7.</summary>
        ASharp7 = 106,
        /// <summary>B in octave 7.</summary>
        B7 = 107,

        /// <summary>C in octave 8, usually the highest key on an 88-key keyboard.</summary>
        C8 = 108,
        /// <summary>C# in octave 8.</summary>
        CSharp8 = 109,
        /// <summary>D in octave 8.</summary>
        D8 = 110,
        /// <summary>D# in octave 8.</summary>
        DSharp8 = 111,
        /// <summary>E in octave 8.</summary>
        E8 = 112,
        /// <summary>F in octave 8.</summary>
        F8 = 113,
        /// <summary>F# in octave 8.</summary>
        FSharp8 = 114,
        /// <summary>G in octave 8.</summary>
        G8 = 115,
        /// <summary>G# in octave 8.</summary>
        GSharp8 = 116,
        /// <summary>A in octave 8.</summary>
        A8 = 117,
        /// <summary>A# in octave 8.</summary>
        ASharp8 = 118,
        /// <summary>B in octave 8.</summary>
        B8 = 119,

        /// <summary>C in octave 9.</summary>
        C9 = 120,
        /// <summary>C# in octave 9.</summary>
        CSharp9 = 121,
        /// <summary>D in octave 9.</summary>
        D9 = 122,
        /// <summary>D# in octave 9.</summary>
        DSharp9 = 123,
        /// <summary>E in octave 9.</summary>
        E9 = 124,
        /// <summary>F in octave 9.</summary>
        F9 = 125,
        /// <summary>F# in octave 9.</summary>
        FSharp9 = 126,
        /// <summary>G in octave 9.</summary>
        G9 = 127
    }
}
