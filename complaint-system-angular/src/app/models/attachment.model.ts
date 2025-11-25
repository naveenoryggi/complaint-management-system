export interface Attachment {
  id: string;
  complaintId: string;
  fileName: string;
  storedFileName: string;
  filePath: string;
  fileSize: number;
  contentType: string;
  fileExtension: string;
  uploadedBy: string;
  uploadedAt: string;
  description?: string;
  isThumbnail: boolean;
}

export interface AttachmentApiResponse {
  isSuccess: boolean;
  message: string;
  data?: Attachment[];
  errors?: string[];
}
