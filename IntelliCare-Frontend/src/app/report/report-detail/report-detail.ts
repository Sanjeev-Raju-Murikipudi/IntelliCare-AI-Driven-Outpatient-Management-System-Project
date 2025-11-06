import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ReportService } from '../../services/report.service';
import { ReportDetailDto } from '../../models/report-detail.dto';
 
@Component({
  selector: 'app-report-detail',
    standalone: false,
 
  templateUrl: './report-detail.html',
//  styleUrls: ['./report-detail.css']
})
export class ReportDetail implements OnInit {
  reportId!: number;
  reportDetail!: ReportDetailDto;
  isLoading = false;
  errorMessage = '';
 
  constructor(private route: ActivatedRoute, private reportService: ReportService) {}
 
  ngOnInit(): void {
    this.reportId = Number(this.route.snapshot.paramMap.get('id'));
    this.fetchReportDetail();
  }
 
 parsedMetrics: any[] = [];
parsedDetails: any = {};
 
fetchReportDetail(): void {
  this.isLoading = true;
  this.reportService.getReportDetail(this.reportId).subscribe({
    next: (res: ReportDetailDto) => {
      this.reportDetail = res;
      this.parsedMetrics = JSON.parse(res.metricsJson);
      console.log('Raw detailedDataJson:', res.detailedDataJson);
 
      try {
        this.parsedDetails = JSON.parse(res.detailedDataJson);
      } catch (err) {
        console.error('Error parsing detailedDataJson:', err);
        this.parsedDetails = {};
      }
 
      // 🔍 Try to extract ForecastBasis manually if missing
      if (!this.parsedDetails.ForecastBasis) {
  this.parsedDetails.ForecastBasis =
    this.parsedDetails.TargetDoctor ||
    this.parsedDetails.SourceDoctor ||
    this.parsedDetails.ModelUsed ||
    null;
}
 
 
      console.log('Parsed  Basis:', this.parsedDetails.ForecastBasis);
      this.isLoading = false;
    },
    error: (err: any) => {
      this.errorMessage = 'Failed to load report detail.';
      console.error('Error fetching report detail:', err);
      this.isLoading = false;
    }
  });
}
 
 
}
 
 