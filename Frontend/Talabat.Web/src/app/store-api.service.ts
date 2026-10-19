import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import {
  AuthToken,
  Basket,
  DeliveryMethod,
  LookupItem,
  OrdersSummary,
  Product,
  ProductPage,
  ShippingAddress,
  StripeConfig,
  UserProfile,
} from './models';

@Injectable({ providedIn: 'root' })
export class StoreApiService {
  private readonly http = inject(HttpClient);
  private readonly api = '/api';

  getProducts(filters: {
    pageIndex: number;
    search?: string;
    brandId?: number;
    typeId?: number;
    sort?: string;
  }) {
    let params = new HttpParams()
      .set('pageIndex', filters.pageIndex)
      .set('pageSize', 10);
    if (filters.search) params = params.set('search', filters.search);
    if (filters.brandId) params = params.set('brandId', filters.brandId);
    if (filters.typeId) params = params.set('typeId', filters.typeId);
    if (filters.sort) params = params.set('sort', filters.sort);
    return this.http.get<ProductPage>(`${this.api}/Product`, { params });
  }

  getBrands() {
    return this.http.get<LookupItem[]>(`${this.api}/Product/brands`);
  }

  getTypes() {
    return this.http.get<LookupItem[]>(`${this.api}/Product/types`);
  }

  getProduct(id: number) {
    return this.http.get<Product>(`${this.api}/Product/${id}`);
  }

  login(email: string, password: string) {
    return this.http.post<AuthToken>(`${this.api}/Account/login`, { email, password });
  }

  register(user: {
    displayName: string;
    email: string;
    userName: string;
    password: string;
    phoneNumber: string;
  }) {
    return this.http.post<AuthToken>(`${this.api}/Account/register`, user);
  }

  getCurrentUser() {
    return this.http.get<UserProfile>(`${this.api}/Account/GetCurrentUser`);
  }

  getUserAddress() {
    return this.http.get<ShippingAddress | null>(`${this.api}/Account/adderss`);
  }

  getBasket(id: string) {
    return this.http.get<Basket>(`${this.api}/Basket/${encodeURIComponent(id)}`);
  }

  saveBasket(basket: Basket) {
    return this.http.post<Basket>(`${this.api}/Basket`, basket);
  }

  deleteBasket(id: string) {
    return this.http.delete<{ success: boolean }>(
      `${this.api}/Basket/${encodeURIComponent(id)}`,
    );
  }

  getDeliveryMethods() {
    return this.http.get<DeliveryMethod[]>(`${this.api}/Order/deliverymethods`);
  }

  getStripeConfig() {
    return this.http.get<StripeConfig>(`${this.api}/PaymentIntent/config`);
  }

  createPaymentIntent(basketId: string) {
    return this.http.post<Basket>(
      `${this.api}/PaymentIntent/${encodeURIComponent(basketId)}`,
      {},
    );
  }

  createOrder(order: {
    deliveryMethodId: number;
    basketId: string;
    shippingAddress: ShippingAddress;
  }) {
    return this.http.post(`${this.api}/Order`, order);
  }

  getOrders(email: string) {
    return this.http.get<OrdersSummary>(`${this.api}/Order`, {
      params: new HttpParams().set('buyerEmail', email),
    });
  }

  getOrder(id: number) {
    return this.http.get(`${this.api}/Order/${id}`);
  }

  getCacheKeys() {
    return this.http.get<string[]>(`${this.api}/CacheKey/live-keys`);
  }
}
