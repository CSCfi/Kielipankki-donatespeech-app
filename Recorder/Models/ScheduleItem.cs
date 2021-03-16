using System;
using System.Collections.Generic;

namespace Recorder.Models
{
    public class ItemTypeValue
    {
        public static readonly string Image = "image";
        public static readonly string LocalImage = "local-image";
        public static readonly string Text = "text";
        public static readonly string Choice = "choice";
        public static readonly string MultiChoice = "multi-choice";
        public static readonly string SuperChoice = "super-choice";
        public static readonly string Video = "video";
        public static readonly string YleVideo = "yle-video";
        public static readonly string Audio = "audio";
        public static readonly string YleAudio = "yle-audio";

        private ItemTypeValue() { }
    }

    public class ItemKindValue
    {
        public static readonly string Media = "media";
        public static readonly string Prompt = "prompt";
    }

    public class ScheduleItem : ICloneable
    {
        public string ItemId;
        public string Kind;
        public string ItemType;
        public string TypeId;
        public string Url;
        public Dictionary<string, string> Title;
        public Dictionary<string, string> Body1;
        public Dictionary<string, string> Body2;
        public Dictionary<string, string> MetaTitle;  // new for v2, v1 clients will ignore this
        public List<Dictionary<string, string>> Options;
        public bool IsRecording;
        public int StartTime;
        public int EndTime;
        public ScheduleItemState Start;
        public ScheduleItemState Recording;
        public ScheduleItemState Finish;
        public Dictionary<string, string> OtherAnswer;  // "other" answer = checkbox label for multi-choice
        public Dictionary<string, string> OtherEntryLabel;  // text entry label for super-choice and multi-choice

        public Dictionary<string, string> StartTitle => Start?.Title ?? Title;
        public Dictionary<string, string> StartBody1 => Start?.Body1 ?? Body1;
        public Dictionary<string, string> StartBody2 => Start?.Body2 ?? Body2;
        public string StartUrl => Start?.ImageUrl ?? Url;

        public Dictionary<string, string> RecordingTitle => Recording?.Title ?? Title;
        public Dictionary<string, string> RecordingBody1 => Recording?.Body1 ?? Body1;
        public Dictionary<string, string> RecordingBody2 => Recording?.Body2 ?? Body2;
        public string RecordingUrl => Recording?.ImageUrl ?? Url;

        // finish title is not defined here since its default comes from resources
        public Dictionary<string, string> FinishBody1 => Finish?.Body1 ?? Body1;
        public Dictionary<string, string> FinishBody2 => Finish?.Body2 ?? Body2;
        public string FinishUrl => Finish?.ImageUrl ?? Url;

        public bool IsPrompt => Kind.Equals(ItemKindValue.Prompt);
        public bool IsMedia => Kind.Equals(ItemKindValue.Media);

        public bool IsChoice => ItemType.Equals(ItemTypeValue.Choice) || ItemType.Equals(ItemTypeValue.MultiChoice);

        public object Clone() => this.MemberwiseClone();
    }
}
