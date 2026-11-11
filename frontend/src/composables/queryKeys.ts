import type { CourseThumbnailQueryParameters, QueryParameters } from '@/types/api'

export const queryKeys = {
  admin: {
    all: ['admin'] as const,
    users: () => [...queryKeys.admin.all, 'users'] as const,
    userList: (params?: QueryParameters) => [...queryKeys.admin.users(), 'list', params ?? {}] as const,
    userDetails: () => [...queryKeys.admin.users(), 'detail'] as const,
    userDetail: (userId: number) => [...queryKeys.admin.userDetails(), userId] as const,
    userSessions: (userId: number, params?: QueryParameters) =>
      [...queryKeys.admin.userDetail(userId), 'sessions', params ?? {}] as const,
  },
  courses: {
    all: ['courses'] as const,
    lists: () => [...queryKeys.courses.all, 'list'] as const,
    list: (params?: QueryParameters) => [...queryKeys.courses.lists(), params ?? {}] as const,
    thumbnails: () => [...queryKeys.courses.all, 'thumbnails'] as const,
    thumbnailList: (params?: CourseThumbnailQueryParameters) =>
      [...queryKeys.courses.thumbnails(), params ?? {}] as const,
    details: () => [...queryKeys.courses.all, 'detail'] as const,
    detail: (id: number) => [...queryKeys.courses.details(), id] as const,
    instructors: (courseId: number, params?: QueryParameters) =>
      [...queryKeys.courses.detail(courseId), 'instructors', params ?? {}] as const,
    sections: (courseId: number, params?: QueryParameters) =>
      [...queryKeys.courses.detail(courseId), 'sections', params ?? {}] as const,
  },
  enrollments: {
    all: ['enrollments'] as const,
    byCourse: (courseId: number, params?: QueryParameters) =>
      [...queryKeys.enrollments.all, 'course', courseId, params ?? {}] as const,
  },
  sections: {
    all: ['sections'] as const,
    details: () => [...queryKeys.sections.all, 'detail'] as const,
    detail: (sectionId: number) => [...queryKeys.sections.details(), sectionId] as const,
    lessons: (sectionId: number, params?: QueryParameters) =>
      [...queryKeys.sections.detail(sectionId), 'lessons', params ?? {}] as const,
    quizzes: (sectionId: number, params?: QueryParameters) =>
      [...queryKeys.sections.detail(sectionId), 'quizzes', params ?? {}] as const,
  },
  lessons: {
    all: ['lessons'] as const,
    details: () => [...queryKeys.lessons.all, 'detail'] as const,
    detail: (lessonId: number) => [...queryKeys.lessons.details(), lessonId] as const,
    attachments: (lessonId: number, params?: QueryParameters) =>
      [...queryKeys.lessons.detail(lessonId), 'attachments', params ?? {}] as const,
  },
  quizzes: {
    all: ['quizzes'] as const,
    details: () => [...queryKeys.quizzes.all, 'detail'] as const,
    detail: (quizId: number) => [...queryKeys.quizzes.details(), quizId] as const,
    editor: (quizId: number) => [...queryKeys.quizzes.all, 'editor', quizId] as const,
    questions: (quizId: number, params?: QueryParameters) =>
      [...queryKeys.quizzes.detail(quizId), 'questions', params ?? {}] as const,
    attempts: (quizId: number, params?: QueryParameters) =>
      [...queryKeys.quizzes.all, quizId, 'attempts', params ?? {}] as const,
    myAttempts: (quizId: number, params?: QueryParameters) =>
      [...queryKeys.quizzes.all, quizId, 'my-attempts', params ?? {}] as const,
    attemptDetail: (attemptId: number) =>
      [...queryKeys.quizzes.all, 'attempt', attemptId] as const,
  },
  questions: {
    all: ['questions'] as const,
    details: () => [...queryKeys.questions.all, 'detail'] as const,
    detail: (questionId: number) => [...queryKeys.questions.details(), questionId] as const,
    options: (questionId: number, params?: QueryParameters) =>
      [...queryKeys.questions.detail(questionId), 'options', params ?? {}] as const,
    optionsEditor: (questionId: number, params?: QueryParameters) =>
      [...queryKeys.questions.detail(questionId), 'options-editor', params ?? {}] as const,
  },
  users: {
    all: ['users'] as const,
    myCourses: (params?: QueryParameters) =>
      [...queryKeys.users.all, 'me', 'courses', params ?? {}] as const,
    myProgress: (params?: QueryParameters) =>
      [...queryKeys.users.all, 'me', 'progress', params ?? {}] as const,
  },
}
