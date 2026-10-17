namespace Domain.Enums;

public enum ProjectType { Scrum, Kanban, Basic }
public enum ProjectStatus { Active, Archived, Paused }
public enum ProjectPrivacy { Private, Public }
public enum SprintStatus { Planned, Active, Completed }
public enum WorkItemType { Epic, Story, Task, Bug, Subtask }
public enum WorkItemStatus { Todo, InProgress, Review, Done }
public enum WorkItemPriority { Lowest, Low, Medium, High, Highest }
public enum MeetingStatus { Scheduled, Completed, Cancelled }
