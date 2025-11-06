import { Component, OnInit } from '@angular/core';
import { ReportService } from '../../services/report.service';
import { Router } from '@angular/router';
import { ReportSummaryDto } from '../../models/report-summary.dto';
 
@Component({
  selector: 'app-report-list',
  standalone: false,
  templateUrl: './report-list.html',
  //styleUrls: ['./report-list.css']
})
export class ReportList implements OnInit {
  allReports: ReportSummaryDto[] = []; // ✅ full list for filtering
  reports: ReportSummaryDto[] = [];    // ✅ filtered list for display
  isLoading = false;
  errorMessage = '';
 
  filterType: string = '';
  filterID: number | null = null;
 
  constructor(private reportService: ReportService, private router: Router) {}
 
  ngOnInit(): void {
    this.fetchReports();
  }
 
  fetchReports(): void {
    this.isLoading = true;
    this.reportService.getAllReports().subscribe({
      next: (res: ReportSummaryDto[]) => {
        this.allReports = res.sort((a, b) => a.reportID - b.reportID); // ✅ store full sorted list
        this.reports = [...this.allReports]; // ✅ show all initially
        this.isLoading = false;
      },
      error: (err: any) => {
        this.errorMessage = 'Failed to load reports.';
        console.error('Error fetching reports:', err);
        this.isLoading = false;
      }
    });
  }
 
  applyFilters(): void {
  this.reports = this.allReports.filter(report => {
    const matchesType = this.filterType
      ? report.type.toUpperCase() === this.filterType.toUpperCase()
      : true;
 
    const matchesID = this.filterID
      ? report.reportID === this.filterID
      : true;
 
    return matchesType && matchesID;
  });
}
 
 
  viewReport(reportID: number): void {
    this.router.navigate(['/report/detail', reportID]);
  }
 
  deleteReport(reportID: number): void {
    if (!confirm('Are you sure you want to delete this report?')) return;
 
    this.reportService.deleteReport(reportID).subscribe({
      next: () => {
        // ✅ remove from allReports and reapply filters
        this.allReports = this.allReports.filter(r => r.reportID !== reportID);
        this.applyFilters();
      },
      error: (err: any) => {
        console.error('Delete failed:', err);
        alert('Failed to delete report.');
      }
    });
  }
}
 
 