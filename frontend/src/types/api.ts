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
  Search?: string
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
}

export interface RegisterRequestDto {
  username: string
  email: string
  password: string
}

export interface RefreshTokenRequestDto {
  accessToken: string
  refreshToken: string
}

export interface LogoutRequestDto {
  refreshToken: string
}

export interface AuthResponseDto {
  accessToken: string
  refreshToken: string
  expiresAtUtc: string
  tokenType?: string
}

// ============================================================================
// Uživatelé a role (Users & RBAC)
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

export interface CreateUserRequestDto {
  username: string
  email: string
  password: string
  role: UserRole | string
}

export type UpdateUserRequestDto = CreateUserRequestDto

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
  accessKey: string | null
}

export interface CourseThumbnailResponseDto {
  id: number
  title: string
  shortDescription: string
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

export interface EnrollmentResponseDto {
  enrolledStudents: UserResponseDto[]
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

// ============================================================================
// Otázky (Questions)
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
  type: QuestionType
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

// ============================================================================
// Typové aliasy pro OpenAPI Wrapper schémata
// ============================================================================

export type WrapperOfUint = ApiWrapper<number>
export type WrapperOfAuthResponseDto = ApiWrapper<AuthResponseDto>
export type WrapperOfCourseResponseDto = ApiWrapper<CourseResponseDto>
export type WrapperOfCourseResponseDtoList = ApiWrapper<CourseResponseDto[]>
export type WrapperOfCourseThumbnailResponseDtoList = ApiWrapper<CourseThumbnailResponseDto[]>
export type WrapperOfEnrollmentResponseDto = ApiWrapper<EnrollmentResponseDto>
export type WrapperOfLessonResponseDto = ApiWrapper<LessonResponseDto>
export type WrapperOfQuestionResponseDto = ApiWrapper<QuestionResponseDto>
export type WrapperOfQuizAttemptResponseDto = ApiWrapper<QuizAttemptResponseDto>
export type WrapperOfQuizAttemptResponseDtoList = ApiWrapper<QuizAttemptResponseDto[]>
export type WrapperOfQuizResponseDto = ApiWrapper<QuizResponseDto>
export type WrapperOfSectionResponseDto = ApiWrapper<SectionResponseDto>
export type WrapperOfUserResponseDto = ApiWrapper<UserResponseDto>
export type WrapperOfUserResponseDtoList = ApiWrapper<UserResponseDto[]>
