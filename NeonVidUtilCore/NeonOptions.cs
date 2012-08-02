using System;
using System.Collections.Generic;

namespace NeonVidUtil.Core {
	public class NeonOptions {
		public NeonOptions() {
			dict = new Dictionary<SettingItem, string>();
			
			this["WAV", "bitdepth"] = "auto";
		}
		
		private Dictionary<SettingItem, string> dict;
		
		public string this[SettingItem item] {
			get { try { return dict[item]; } catch { return null; } }
			set { AddOrSet(item, value); }
		}
		
		public string this[string plugin, string name] {
			get { return this[new SettingItem(plugin, name)]; }
			set { this[new SettingItem(plugin, name)] = value; }
		}
		
		public string this[FormatHandler plugin, string name] {
			get {
				string pluginName = null;
				if(plugin == null) {
					return null;
				}
				else if((pluginName = PluginHelper.PluginShortName(plugin.GetType().Name)) != null) {
					return this[pluginName, name];
				}
				else {
					return null;
				}
			}
			set {
				string pluginName = null;
				if(plugin != null && (pluginName = PluginHelper.PluginShortName(plugin.GetType().Name)) != null) {
					this[pluginName, name] = value;
				}
			}
		}
		
		public void Add(SettingItem key, string val) {
			if(!dict.ContainsKey(key)) {
				dict.Add(key, val);
			}
		}
		
		private void AddOrSet(SettingItem key, string val) {
			if(!dict.ContainsKey(key)) {
				dict.Add(key, val);
			}
			else {
				dict[key] = val;
			}
		}
		
		public static bool GetBoolValue(string val) {
			return val == "1" || val.ToUpper() == "TRUE" || val.ToUpper() == "YES";
		}
		
		public class SettingItem {
			public SettingItem(string str) {
				string[] parts = str.Split(new char[] { ':' }, 2);
				if(parts.Length != 2) throw new ArgumentException("No value to set.");
				
				PluginName = parts[0];
				SettingName = parts[1];
			}
			
			public SettingItem(string plugin, string name) {
				PluginName = plugin;
				SettingName = name;
			}
			
			public string PluginName {
				get;
				protected set;
			}
			
			public string SettingName {
				get;
				protected set;
			}
			
			public override bool Equals(object obj) {
				if(!(obj is SettingItem)) {
					return false;
				}
				
				return Equals((SettingItem)obj);
			}
			
			public bool Equals(SettingItem item) {
				return PluginName == item.PluginName && SettingName == item.SettingName;
			}
			
			public static bool operator==(SettingItem s1, SettingItem s2) {
				return s1.Equals(s2);
			}
			
			public static bool operator!=(SettingItem s1, SettingItem s2) {
				return !s1.Equals(s2);
			}
			
			public override int GetHashCode() {
				return 7*PluginName.GetHashCode() + 17*SettingName.GetHashCode();
			}
			
			public override string ToString() {
				return string.Format("{0}:{1}", PluginName, SettingName);
			}
		}
	}
}

