// ============================================================================
// Společné obálky a metadata (Wrapper & Metadata)
// ============================================================================

export interface PaginationMetadata {
  page: number
  pageSize: number
  totalItems: number
  totalPages: number
  hasNextPage: boolean
  hasPreviousPage: boolean
}

export interface SortingMetadata {
  sortBy: string | null
  isDescending: boolean
}

export interface FilterMetadata {
  appliedFilters?: Record<string, string>
}

export interface ServerMetadata {
  timestamp: string
  executionTimeMs: number
}

export interface Metadata {
  pagination?: PaginationMetadata | null
  sorting?: SortingMetadata | null
  filters?: FilterMetadata | null
  serverInfo?: ServerMetadata | null
}

export interface ApiWrapper<T> {
  data: T
  metadata?: Metadata | null
}

export interface ProblemDetails {
  type?: string | null
  title?: string | null
  status?: number | null
  detail?: string | null
  instance?: string | null
  errors?: Record<string, string[]>
  [key: string]: unknown
}

// ============================================================================
// Dotazovací parametry (Query Parameters)
// ============================================================================

export interface QueryParameters {
  Page?: number
  PageSize?: number
  SortBy?: string
  Descending?: boolean
  Filters?: string | Record<string, unknown>
}

export interface CourseThumbnailQueryParameters extends QueryParameters {
  MaxDescriptionLength?: number
}

// ============================================================================
// Autentizace (Authentication)
// ============================================================================

export interface LoginRequestDto {
  username?: string | null
  email?: string | null
  password: string
  deviceName?: string | null
}

export interface RegisterRequestDto {
  username: string
  email: string
  password: string
  deviceName?: string | null
}

export interface AuthResponseDto {
  accessToken: string
  expiresAtUtc: string
  tokenType?: string
}

// ============================================================================
// Uživatelé, role a administrace (Users, RBAC & Administration)
// ============================================================================

export const UserRoles = {
  Admin: 'Admin',
  Instructor: 'Instructor',
  Student: 'Student',
} as const

export type UserRole = (typeof UserRoles)[keyof typeof UserRoles]

export interface UserResponseDto {
  id: number
  username: string
  email: string
  role: UserRole | string
}

export interface UpdateUserRequestDto {
  username: string
  email: string
  password: string
}

export interface UpdateUserRoleRequestDto {
  role: UserRole
}

export interface AdminUserDetailsDto {
  id: number
  username: string
  email: string
  role: UserRole | string
  enrollmentCount: number
  lessonProgressCount: number
  quizAttemptCount: number
  activeSessionCount: number
}

export interface AdminSessionResponseDto {
  sessionId: string
  deviceName: string | null
  userAgent: string | null
  createdByIp: string | null
  createdAtUtc: string
  expiresAtUtc: string
}

export interface UserCourseResponseDto {
  id: number
  title: string
  shortDescription: string
}

export interface UserLessonProgressResponseDto {
  lessonId: number
  lessonTitle: string
  sectionId: number
  courseId: number
  isCompleted: boolean
  completedAtUtc: string | null
}

// ============================================================================
// Kurzy (Courses)
// ============================================================================

export interface CourseCreateRequestDto {
  title: string
  description: string
  accessKey: string | null
}

export interface CourseUpdateRequestDto {
  title: string
  description: string
  accessKey: string | null
}

export interface CourseResponseDto {
  id: number
  title: string
  description: string
}

export interface CourseThumbnailResponseDto {
  id: number
  title: string
  shortDescription: string
}

export interface CourseInstructorResponseDto {
  id: number
  username: string
}

// ============================================================================
// Zápisy do kurzů (Enrollments)
// ============================================================================

export interface EnrollmentRequestDto {
  courseId: number
  accessKey: string | null
}

export interface ForceEnrollmentRequestDto {
  studentId: number
  courseId: number
}

// ============================================================================
// Sekce (Sections)
// ============================================================================

export interface CreateSectionRequestDto {
  title: string
  order: number
  courseId: number
}

export type UpdateSectionRequestDto = CreateSectionRequestDto

export interface SectionResponseDto {
  id: number
  title: string
  order: number
  courseId: number
}

// ============================================================================
// Lekce a přílohy (Lessons & Attachments)
// ============================================================================

export interface CreateLessonRequestDto {
  title: string
  content: string
  videoUrl: string | null
  order: number
  sectionId: number
}

export type UpdateLessonRequestDto = CreateLessonRequestDto

export interface LessonResponseDto {
  id: number
  title: string
  content: string
  videoUrl: string | null
  order: number
  sectionId: number
}

export interface UpdateProgressDto {
  isCompleted: boolean
}

export interface AttachmentRequestDto {
  lessonId: number
  fileName: string
  url: string
  contentType: string | null
  sizeBytes: number | null
}

export interface AttachmentResponseDto {
  id: number
  fileName: string
  url: string
  contentType: string | null
  sizeBytes: number | null
  lessonId: number
}

// ============================================================================
// Kvízy a pokusy (Quizzes & Quiz Attempts)
// ============================================================================

export interface CreateQuizRequestDto {
  title: string
  sectionId: number
  maxAttempts: number | null
  timeLimitSeconds: number | null
  passingThresholdPercent: number
  randomizeQuestions: boolean
  randomizeAnswers: boolean
}

export type UpdateQuizRequestDto = CreateQuizRequestDto

export interface QuizResponseDto {
  id: number
  title: string
  maxAttempts: number | null
  timeLimitSeconds: number | null
  passingThresholdPercent: number
  sectionId: number
}

export interface StartAttemptRequestDto {
  quizId: number
}

export interface EndAttemptRequestDto {
  attemptId: number
  answersJson: string
}

export interface QuizAttemptResponseDto {
  id: number
  quizId: number
  studentId: number
  startedAtUtc: string
  submittedAtUtc: string | null
  scorePercent: number
  isPassed: boolean
}

export interface MyQuizAttemptResponseDto {
  id: number
  startedAtUtc: string
  submittedAtUtc: string | null
  scorePercent: number
  isPassed: boolean
}

// ============================================================================
// Otázky a možnosti odpovědí (Questions & Answer Options)
// ============================================================================

export const QuestionTypes = {
  SingleChoice: 'SingleChoice',
  MultipleChoice: 'MultipleChoice',
  TrueFalse: 'TrueFalse',
} as const

export type QuestionType = (typeof QuestionTypes)[keyof typeof QuestionTypes]

export interface CreateQuestionRequestDto {
  text: string
  imageUrl: string | null
  type: QuestionType | string
  points: number
  quizId: number
}

export type UpdateQuestionRequestDto = CreateQuestionRequestDto

export interface QuestionResponseDto {
  id: number
  text: string
  imageUrl: string | null
  type: QuestionType
  points: number
  quizId: number
}

export interface CreateAnswerOptionRequestDto {
  text: string
  imageUrl: string | null
  isCorrect: boolean
}

export interface UpdateAnswerOptionRequestDto {
  text: string
  imageUrl: string | null
  isCorrect: boolean
}

export interface AnswerOptionResponseDto {
  id: number
  text: string
  imageUrl: string | null
  questionId: number
}

export interface AnswerOptionEditorResponseDto {
  id: number
  text: string
  imageUrl: string | null
  isCorrect: boolean
  questionId: number
}

// ============================================================================
// Typové aliasy pro OpenAPI Wrapper schémata
// ============================================================================

export type WrapperOfUint = ApiWrapper<number>
export type WrapperOfAdminSessionResponseDtoList = ApiWrapper<AdminSessionResponseDto[]>
export type WrapperOfAdminUserDetailsDto = ApiWrapper<AdminUserDetailsDto>
export type WrapperOfAnswerOptionEditorResponseDtoList = ApiWrapper<AnswerOptionEditorResponseDto[]>
export type WrapperOfAnswerOptionResponseDtoList = ApiWrapper<AnswerOptionResponseDto[]>
export type WrapperOfAttachmentResponseDtoList = ApiWrapper<AttachmentResponseDto[]>
export type WrapperOfAuthResponseDto = ApiWrapper<AuthResponseDto>
export type WrapperOfCourseInstructorResponseDtoList = ApiWrapper<CourseInstructorResponseDto[]>
export type WrapperOfCourseResponseDto = ApiWrapper<CourseResponseDto>
export type WrapperOfCourseResponseDtoList = ApiWrapper<CourseResponseDto[]>
export type WrapperOfCourseThumbnailResponseDtoList = ApiWrapper<CourseThumbnailResponseDto[]>
export type WrapperOfLessonResponseDto = ApiWrapper<LessonResponseDto>
export type WrapperOfLessonResponseDtoList = ApiWrapper<LessonResponseDto[]>
export type WrapperOfMyQuizAttemptResponseDtoList = ApiWrapper<MyQuizAttemptResponseDto[]>
export type WrapperOfQuestionResponseDto = ApiWrapper<QuestionResponseDto>
export type WrapperOfQuestionResponseDtoList = ApiWrapper<QuestionResponseDto[]>
export type WrapperOfQuizAttemptResponseDto = ApiWrapper<QuizAttemptResponseDto>
export type WrapperOfQuizAttemptResponseDtoList = ApiWrapper<QuizAttemptResponseDto[]>
export type WrapperOfQuizResponseDto = ApiWrapper<QuizResponseDto>
export type WrapperOfQuizResponseDtoList = ApiWrapper<QuizResponseDto[]>
export type WrapperOfSectionResponseDto = ApiWrapper<SectionResponseDto>
export type WrapperOfSectionResponseDtoList = ApiWrapper<SectionResponseDto[]>
export type WrapperOfUserCourseResponseDtoList = ApiWrapper<UserCourseResponseDto[]>
export type WrapperOfUserLessonProgressResponseDtoList = ApiWrapper<UserLessonProgressResponseDto[]>
export type WrapperOfUserResponseDto = ApiWrapper<UserResponseDto>
export type WrapperOfUserResponseDtoList = ApiWrapper<UserResponseDto[]>
