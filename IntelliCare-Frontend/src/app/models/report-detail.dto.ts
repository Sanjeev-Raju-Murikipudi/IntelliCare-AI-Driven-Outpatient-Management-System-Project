export interface ReportDetailDto {
  reportID: number;
  type: string;
  generatedDate: string;       // ISO string from backend
  metricsJson: string;         // raw JSON string
  detailedDataJson: string;    // raw JSON string
}
 
 