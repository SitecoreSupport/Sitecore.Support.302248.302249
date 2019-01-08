# Sitecore.Support.302248.302249
RefreshTree command can break indexing. At that moment Sitecore log can contain the similar errors: 
```
ManagedPoolThread #14 18:58:32 ERROR One or more exceptions occurred while processing the subscribers to the 'indexing:updateditem' event.
Exception[1]: System.IndexOutOfRangeException 
Message[1]: Index was outside the bounds of the array. 
Source[1]: mscorlib 
   at System.Collections.ArrayList.Add(Object value)
   at Sitecore.ContentSearch.Client.Commands.RefreshTree.ShowProgress(Object sender, EventArgs eventArgs)
   at Sitecore.Events.Event.EventSubscribers.RaiseEvent(String eventName, Object[] parameters, EventResult result) 
```
 

## License  
This patch is licensed under the [Sitecore Corporation A/S License for GitHub](https://github.com/sitecoresupport/Sitecore.Support.302248.302249/blob/master/LICENSE).  

## Download  
Downloads are available via [GitHub Releases](https://github.com/sitecoresupport/Sitecore.Support.302248.302249/releases).  
