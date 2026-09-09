import { DatePipe, NgClass } from '@angular/common';
import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { readApiError } from '../../shared/api-error';
import { OrderApiService } from '../order-api.service';
import { Order, OrderStatus, formatMoney, nextStatuses, statusClass } from '../order.model';

@Component({
  selector: 'app-order-detail',
  standalone: true,
  imports: [DatePipe, NgClass, RouterLink],
  templateUrl: './order-detail.component.html',
  styleUrl: './order-detail.component.css'
})
export class OrderDetailComponent implements OnInit {
  private readonly api = inject(OrderApiService);
  private readonly route = inject(ActivatedRoute);

  order: Order | null = null;
  loading = true;
  error = '';
  notice = '';
  updating = false;
  readonly statusClass = statusClass;
  readonly formatMoney = formatMoney;

  ngOnInit(): void {
    if (history.state?.['replay']) {
      this.notice = 'This client reference was already on file, so the original food order is shown instead of creating a duplicate.';
    }

    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.error = 'Missing food order id.';
      this.loading = false;
      return;
    }

    this.api.get(id).subscribe({
      next: (order) => {
        this.order = order;
        this.loading = false;
      },
      error: (err) => {
        this.error = readApiError(err);
        this.loading = false;
      }
    });
  }

  moves(): OrderStatus[] {
    return this.order ? nextStatuses(this.order.status) : [];
  }

  setStatus(status: OrderStatus): void {
    if (!this.order) {
      return;
    }

    this.error = '';
    this.updating = true;
    this.api.changeStatus(this.order.id, status).subscribe({
      next: (order) => {
        this.order = order;
        this.updating = false;
        this.notice = `Status is now ${order.status}.`;
      },
      error: (err) => {
        this.updating = false;
        this.error = readApiError(err);
      }
    });
  }
}
