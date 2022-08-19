using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PatreonResponse
{
	
	[Serializable]
    public class Root
    {
        public List<Campaign> data;
        public List<Included> included;
        public Meta meta;
		
		public string[] _campaign_name_list;
		
		public static Root CreateFromJSON(string jsonString){
			Root r = JsonUtility.FromJson<Root>(jsonString);
			
			List<string> campaign_names = new List<string>();
			foreach(Campaign c in r.data){
				campaign_names.Add( c.attributes.creation_name);
				c.initialize_tier_names(r.included);
			}
			r._campaign_name_list = campaign_names.ToArray();
			
			return r;
			
		}
		
		public Campaign getCampaign(int index){
			return data[index];
		}
		
		public string getUserID(){
			foreach(Included i in included){
				if( i.type == "user" ){
					return i.id;
				}
			}
			
			return "";
		}
    }
	
	[Serializable]
	public class Campaign
	{
		public string type;
		public string id;
		public Attributes attributes;
		public Relationships relationships;
		
		public string[] _tier_name_list;
		
		public void initialize_tier_names(List<Included> included){
			List<string> tier_names = new List<string>();
			
			foreach(Data tier in relationships.tiers.data){
				if( tier.type == "tier" ){
					
					// The slow way because I'm lazy
					foreach(Included i in included){
						if(i.id == tier.id){
							tier_names.Add( i.attributes.title );
							break;
						}
					}
				}
			}
			
			_tier_name_list = tier_names.ToArray();
		}
		
		public string getName(){
			return attributes.creation_name;
		}
		
		public string getTierID(int index){
			return relationships.tiers.data[index].id;
		}
		
	}

	[Serializable]
	public class Attributes
    {
        public string creation_name;
        public string title;
    }
	
	[Serializable]
    public class Creator
    {
        public Data data;
        public Links links;
    }
	
	[Serializable]
    public class Data
    {
        public string id;
        public string type;
        public Attributes attributes;
    }
	
	[Serializable]
    public class Included
    {
        public Attributes attributes;
        public string id;
        public string type;
    }
	
	[Serializable]
    public class Links
    {
        public string related;
    }
	
	[Serializable]
    public class Meta
    {
        public Pagination pagination;
    }
	
	[Serializable]
    public class Pagination
    {
        public int total;
    }
	
	[Serializable]
    public class Relationships
    {
        public Creator creator;
        public Tiers tiers;
    }
	
	[Serializable]
    public class Tiers
    {
        public List<Data> data;
    }
}
