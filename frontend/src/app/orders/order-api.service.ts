import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { CreateOrderPayload, Order, OrderStatus } from './order.model';

export interface SubmitResult {
  order: Order;
  created: boolean;
}

@Injectable({ providedIn: 'root' })
export class OrderApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = '/api/orders';

  list(): Observable<Order[]> {
    return this.http.get<Order[]>(this.baseUrl);
  }

  get(id: string): Observable<Order> {
    return this.http.get<Order>(`${this.baseUrl}/${id}`);
  }

  submit(payload: CreateOrderPayload): Observable<SubmitResult> {
    return this.http.post<Order>(this.baseUrl, payload, { observe: 'response' }).pipe(
      map((res) => ({
        order: res.body as Order,
        created: res.status === 201
      }))
    );
  }

  changeStatus(id: string, status: OrderStatus): Observable<Order> {
    return this.http.post<Order>(`${this.baseUrl}/${id}/status`, { status });
  }
}

export function readApiError(err: unknown): string {
  if (!(err instanceof HttpErrorResponse)) {
    return 'Something went wrong.';
  }

  const body = err.error;
  if (body?.errors?.length) {
    return body.errors.join(' ');
  }
  if (typeof body?.detail === 'string' && body.detail) {
    return body.detail;
  }
  if (err.status === 0) {
    return 'Cannot reach the API. Is the backend running on port 5080?';
  }
  return err.message || 'Request failed.';
}
