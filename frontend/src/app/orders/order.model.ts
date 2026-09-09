export type OrderStatus = 'Pending' | 'Confirmed' | 'Ready' | 'Done' | 'Cancelled';

export interface MenuItem {
  id: string;
  code: string;
  name: string;
  unitPrice: number;
}

export interface LineItem {
  id: string;
  menuItemId: string;
  menuCode: string;
  name: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  clientReference?: string | null;
  lineItems: LineItem[];
  notes?: string | null;
  subtotal: number;
  total: number;
  status: OrderStatus;
  createdAt: string;
  updatedAt: string;
}

export interface CreateOrderPayload {
  clientReference?: string;
  notes?: string;
  lineItems: { menuItemId: string; quantity: number }[];
}

export const STATUS_OPTIONS: OrderStatus[] = [
  'Pending',
  'Confirmed',
  'Ready',
  'Done',
  'Cancelled'
];

export function nextStatuses(current: OrderStatus): OrderStatus[] {
  switch (current) {
    case 'Pending':
      return ['Confirmed', 'Cancelled'];
    case 'Confirmed':
      return ['Ready', 'Cancelled'];
    case 'Ready':
      return ['Done'];
    default:
      return [];
  }
}

export function formatMoney(amount: number): string {
  return 'R ' + amount.toFixed(2);
}

export function statusClass(status: OrderStatus): string {
  return 'st-' + status.toLowerCase();
}
