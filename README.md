# BugTracker-FinalProject
 
## The New Requirement
- make a new class **TicketLogItem** class and have a one to one relationship   between TicketHistory and TicketLogItem.
- TicketHistory has a collection of strings of properties changed. e.g ("Title (newTitle), Priority (newPriority)"). 

 ## Adjustment to Project and Database
- Had to replace properties **NewValue**,**OldValue** and **Property** with a single property of ICollection<`string`>() PropertiesChanged.

- Added a new one to one relationship with modelBuilder between TicketHistory and TicketLogItem.