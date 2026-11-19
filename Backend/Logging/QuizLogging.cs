namespace Backend.Logging;

internal static partial class QuizLog {
    [LoggerMessage(
        EventId = 2001,
        Level = LogLevel.Debug,
        Message = "HTTP action {ActionName} invoked by user {UserId} (TraceId: {TraceId})")]
    public static partial void EndpointInvoked(this ILogger logger, string actionName, uint userId, string traceId);

    [LoggerMessage(
        EventId = 2002,
        Level = LogLevel.Information,
        Message = "User {UserId} created quiz {QuizId} in section {SectionId}")]
    public static partial void QuizCreated(this ILogger logger, uint userId, uint quizId, uint sectionId);

    [LoggerMessage(
        EventId = 2003,
        Level = LogLevel.Information,
        Message = "User {UserId} updated quiz {QuizId} (SectionId: {SectionId})")]
    public static partial void QuizUpdated(this ILogger logger, uint userId, uint quizId, uint sectionId);

    [LoggerMessage(
        EventId = 2004,
        Level = LogLevel.Information,
        Message = "User {UserId} deleted quiz {QuizId}")]
    public static partial void QuizDeleted(this ILogger logger, uint userId, uint quizId);

    [LoggerMessage(
        EventId = 2005,
        Level = LogLevel.Information,
        Message = "Student {StudentId} started attempt {AttemptId} for quiz {QuizId}")]
    public static partial void AttemptStarted(this ILogger logger, uint studentId, uint attemptId, uint quizId);

    [LoggerMessage(
        EventId = 2006,
        Level = LogLevel.Warning,
        Message = "Auto-closed expired unsubmitted attempt {AttemptId} for student {StudentId} on quiz {QuizId}")]
    public static partial void ExpiredAttemptAutoClosed(this ILogger logger, uint attemptId, uint studentId, uint quizId);

    [LoggerMessage(
        EventId = 2007,
        Level = LogLevel.Warning,
        Message = "Blocked new attempt for student {StudentId} on quiz {QuizId}: an active attempt is already in progress")]
    public static partial void ActiveAttemptConflict(this ILogger logger, uint studentId, uint quizId);

    [LoggerMessage(
        EventId = 2008,
        Level = LogLevel.Warning,
        Message = "Blocked attempt for student {StudentId} on quiz {QuizId}: max attempts ({MaxAttempts}) reached")]
    public static partial void MaxAttemptsReached(this ILogger logger, uint studentId, uint quizId, ushort maxAttempts);

    [LoggerMessage(
        EventId = 2009,
        Level = LogLevel.Information,
        Message = "Student {StudentId} submitted attempt {AttemptId} for quiz {QuizId} with score {ScorePercent}% (Passed: {IsPassed})")]
    public static partial void AttemptSubmitted(this ILogger logger, uint studentId, uint attemptId, uint quizId, decimal scorePercent, bool isPassed);

    [LoggerMessage(
        EventId = 2010,
        Level = LogLevel.Warning,
        Message = "Rejected late submission of attempt {AttemptId} by student {StudentId} on quiz {QuizId}: time limit exceeded")]
    public static partial void AttemptTimeLimitExceeded(this ILogger logger, uint attemptId, uint studentId, uint quizId);

    [LoggerMessage(
        EventId = 2011,
        Level = LogLevel.Warning,
        Message = "Rejected invalid submission payload for attempt {AttemptId} by student {StudentId} (Reason: {Reason})")]
    public static partial void InvalidAttemptSubmission(this ILogger logger, uint attemptId, uint studentId, string reason);
}