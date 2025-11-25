export enum AdvancedAssignmentMethod {
  Manual = 0,
  RoundRobin = 1,
  LeastBusy = 2,
  SkillBased = 3,
  PriorityBased = 4,
  WorkloadBalanced = 5,
  BestFit = 6,
  ExperienceBased = 7,
  Random = 8
}

export interface AssignmentContext {
  assignedByUserId?: string;
  assignmentTimestamp?: Date;
  isInitialAssignment?: boolean;
  isEscalation?: boolean;
  currentEscalationLevel?: number;
  assignmentReason?: string;
  assignmentNotes?: string;
  bypassRules?: boolean;
  overrideAssignmentMethod?: string;
  forceAssignment?: boolean;
  priority?: number;
}

export interface AssignmentCriteria {
  organizationalScope: OrganizationalScope;
  complaintCriteria: ComplaintCriteria;
  workloadCriteria: WorkloadCriteria;
  skillCriteria: SkillCriteria;
  temporalCriteria: TemporalCriteria;
  maxCandidates?: number;
}

export interface OrganizationalScope {
  companyIds?: string[];
  branchIds?: string[];
  departmentIds?: string[];
  sectionIds?: string[];
}

export interface ComplaintCriteria {
  categoryIds?: string[];
  priorityRange: PriorityRange;
  escalationLevelRange: EscalationLevelRange;
}

export interface WorkloadCriteria {
  maxCurrentWorkload?: number;
  minSuccessRate?: number;
  maxAverageResolutionTime?: string; // ISO duration format
  requireRecentActivityDays?: number;
  includeOnLeave?: boolean;
}

export interface SkillCriteria {
  requiredSkills?: string[];
  minSkillLevel?: number;
  preferredSkills?: string[];
  skillWeight?: number;
}

export interface TemporalCriteria {
  businessHoursOnly?: boolean;
  allowedDaysOfWeek?: number[];
  allowedTimeRange?: TimeRange;
  considerUserTimeZone?: boolean;
}

export interface PriorityRange {
  min: number;
  max: number;
}

export interface EscalationLevelRange {
  min: number;
  max: number;
}

export interface TimeRange {
  start: string; // HH:mm format
  end: string;   // HH:mm format
}

// Request DTOs
export interface AutoAssignRequest {
  forceAssignment?: boolean;
  criteria?: AssignmentCriteriaRequest;
  assignmentReason?: string;
  bypassRules?: boolean;
  overrideAssignmentMethod?: string;
  notes?: string;
}

export interface AssignToPoolRequest {
  resourcePoolId: string;
  assignmentMethod?: AdvancedAssignmentMethod;
  specificUserId?: string;
  validateBeforeAssignment?: boolean;
  assignmentReason?: string;
  isEscalation?: boolean;
  bypassRules?: boolean;
  forceAssignment?: boolean;
  notes?: string;
}

export interface ReassignRequest {
  currentAssignedUserId?: string;
  newAssignedUserId?: string;
  newResourcePoolId?: string;
  reassignmentReason: string;
  assignmentMethod?: AdvancedAssignmentMethod;
  keepAssignmentHistory?: boolean;
  bypassRules?: boolean;
  notes?: string;
}

export interface ValidateAssignmentRequest {
  userId?: string;
  poolId?: string;
  isEscalation?: boolean;
  forceAssignment?: boolean;
  detailedValidation?: boolean;
}

export interface AssignmentCriteriaRequest {
  // Organizational scope
  companyIds?: string[];
  branchIds?: string[];
  departmentIds?: string[];
  sectionIds?: string[];

  // Complaint criteria
  categoryIds?: string[];
  minPriorityLevel?: number;
  maxPriorityLevel?: number;
  minEscalationLevel?: number;
  maxEscalationLevel?: number;

  // Workload criteria
  maxCurrentWorkload?: number;
  minSuccessRate?: number;
  maxAverageResolutionTime?: string;
  requireRecentActivityDays?: number;
  includeOnLeave?: boolean;

  // Skill criteria
  requiredSkills?: string[];
  minSkillLevel?: number;
  preferredSkills?: string[];
  skillWeight?: number;

  // Temporal criteria
  businessHoursOnly?: boolean;
  allowedDaysOfWeek?: number[];
  allowedTimeStart?: string;
  allowedTimeEnd?: string;
  considerUserTimeZone?: boolean;

  // Result limits
  maxCandidates?: number;
}

// Result DTOs
export interface AssignmentResult {
  success: boolean;
  assignedUserId?: string;
  assignedUserName?: string;
  resourcePoolId?: string;
  resourcePoolName?: string;
  assignmentMethod: string;
  appliedRuleId?: string;
  appliedRuleName?: string;
  message: string;
  errorCode?: string;
  metadata: Record<string, any>;
  consideredCandidates: ResourcePoolCandidate[];
  executionTimeMs: number;
  assignedAt: Date;
}

export interface ResourcePoolCandidate {
  resourcePoolId: string;
  resourcePoolName: string;
  poolType: string;
  suitabilityScore: number;
  availableMemberCount: number;
  totalMemberCount: number;
  averageWorkload: number;
  successRate: number;
  averageResolutionTime: string; // ISO duration format
  canHandleComplaint: boolean;
  disqualificationReasons: string[];
  matchingSpecializations: PoolSpecializationInfo[];
  recommendedAssignmentMethod: string;
  bestUserCandidate?: UserCandidate;
}

export interface PoolSpecializationInfo {
  categoryId: string;
  categoryName: string;
  minPriorityLevel: number;
  maxPriorityLevel: number;
  weight: number;
}

export interface UserCandidate {
  userId: string;
  fullName: string;
  email: string;
  currentWorkload: number;
  successRate: number;
  averageResolutionTime: string; // ISO duration format
  skillMatchScore: number;
  overallScore: number;
  isAvailable: boolean;
  disqualificationReasons: string[];
  matchingSkills: MatchingSkill[];
  lastActivityAt?: Date;
}

export interface MatchingSkill {
  skillCode: string;
  skillLevel: number;
  isRequired: boolean;
  matchScore: number;
}

export interface AssignmentValidationResult {
  isValid: boolean;
  validationErrors: string[];
  validationWarnings: string[];
  suitabilityScore: number;
  recommendations: string[];
  metadata: Record<string, any>;
}

// Options for getting assignment candidates
export interface AssignmentCandidateOptions {
  maxCandidates?: number;
  includeUserDetails?: boolean;
  calculateSkillScores?: boolean;
}

// Utility functions
export function getAdvancedAssignmentMethodLabel(method: AdvancedAssignmentMethod): string {
  switch (method) {
    case AdvancedAssignmentMethod.Manual:
      return 'Manual';
    case AdvancedAssignmentMethod.RoundRobin:
      return 'Round Robin';
    case AdvancedAssignmentMethod.LeastBusy:
      return 'Least Busy';
    case AdvancedAssignmentMethod.SkillBased:
      return 'Skill Based';
    case AdvancedAssignmentMethod.PriorityBased:
      return 'Priority Based';
    case AdvancedAssignmentMethod.WorkloadBalanced:
      return 'Workload Balanced';
    case AdvancedAssignmentMethod.BestFit:
      return 'Best Fit';
    case AdvancedAssignmentMethod.ExperienceBased:
      return 'Experience Based';
    case AdvancedAssignmentMethod.Random:
      return 'Random';
    default:
      return 'Unknown';
  }
}

export function getAdvancedAssignmentMethodDescription(method: AdvancedAssignmentMethod): string {
  switch (method) {
    case AdvancedAssignmentMethod.Manual:
      return 'Pool members pick up complaints themselves';
    case AdvancedAssignmentMethod.RoundRobin:
      return 'Distribute evenly among available members';
    case AdvancedAssignmentMethod.LeastBusy:
      return 'Assign to member with fewest active complaints';
    case AdvancedAssignmentMethod.SkillBased:
      return 'Assign to member with highest skill level for the complaint type';
    case AdvancedAssignmentMethod.PriorityBased:
      return 'High priority complaints go to most skilled members';
    case AdvancedAssignmentMethod.WorkloadBalanced:
      return 'Advanced load balancing considering multiple factors';
    case AdvancedAssignmentMethod.BestFit:
      return 'Algorithm-based best match considering all factors';
    case AdvancedAssignmentMethod.ExperienceBased:
      return 'Assign to member with previous experience with similar complaints';
    case AdvancedAssignmentMethod.Random:
      return 'Random assignment among qualified members';
    default:
      return 'Unknown assignment method';
  }
}