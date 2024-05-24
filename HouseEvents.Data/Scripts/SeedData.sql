use HouseEvent
go

delete from dbo.House
go

insert into dbo.House (HouseName, UndermasterFirstName, EventsCoordinator)
values ('Harrison', 'Glenn', 'Natalie Dillon')
go

insert into dbo.House (HouseName, UndermasterFirstName, EventsCoordinator)
values ('Field', 'Sarah', 'Sonya Milanova')
go

insert into dbo.House (HouseName, UndermasterFirstName, EventsCoordinator)
values ('Cloete', 'Kerilynne', 'Fergus Wishart')
go

insert into dbo.House (HouseName, UndermasterFirstName, EventsCoordinator)
values ('Gilks', 'James', null)

insert into dbo.House (HouseName, UndermasterFirstName, EventsCoordinator)
values ('Blurton', 'James', null)
go

insert into dbo.House (HouseName, UndermasterFirstName, EventsCoordinator)
values ('Gill', 'Caroline', null)
go

insert into dbo.House (HouseName, UndermasterFirstName, EventsCoordinator)
values ('Nilsson', 'Hugo', null)
go

insert into dbo.House (HouseName, UndermasterFirstName, EventsCoordinator)
values ('Warner', 'Hannah', null)
go

SELECT HouseName, UndermasterFirstName, EventsCoordinator from dbo.House

delete from [dbo].[Event]
go

insert into dbo.[Event] (EventName, SchoolYear) 
values ('House Science', 2023)

delete from [dbo].[EventDetail]
go

insert into dbo.EventDetail (EventID, EventDate, EventStartTime, EventEndTime, EventVenue, Notes)
values (1, '2024-06-21', '13:15', '13:45', 'C4', 'Please note that the content is predominantly chemistry-based. The L8 should be studying chemistry A Level.')
go

delete from dbo.HouseEvent
go

insert into dbo.HouseEvent (EventDetailId, HouseID, Points) 
select e.EventDetailID, h.HouseID, 0 from dbo.EventDetail e cross join dbo.House h
go

delete from dbo.EventParticipant
go

insert into dbo.EventParticipant (HouseEventId, YearGroup, Reserve)
select HouseEventId, '4th/5th', 0 from dbo.HouseEvent
union
select HouseEventId, '4th/5th', 1 from dbo.HouseEvent
union
select HouseEventId, 'L8th', 0 from dbo.HouseEvent
union
select HouseEventId, 'L8th', 1 from dbo.HouseEvent


select EventName, EventDetailId, EventDate, EventStartTime,
EventEndTime, EventVenue, Notes, HouseName, Points, EventParticipantId, 
YearGroup, Reserve, StudentName, NoShow 
from [dbo].[vwEventParticipantsNoFixture]
order by EventId, HouseId, Reserve, YearGroup




