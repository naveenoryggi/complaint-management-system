export interface Comment {
  id: string;
  complaintId: string;
  userId: string;
  userName: string;
  userEmail: string;
  userPhone?: string;
  branchName?: string;
  departmentName?: string;
  sectionName?: string;
  comment: string;
  isInternal: boolean;
  createdAt: string;
}

export interface CreateCommentRequest {
  comment: string;
  isInternal: boolean;
}
