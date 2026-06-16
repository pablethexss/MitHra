import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

export interface Producto {
  id?: number;
  codigo: string;
  codigoBarras: string;
  nombre: string;
  tipo: string;
  stock: number;
  stockMinimo: number;
  precioNeto: number;
  esAfectoIVA: boolean;
  requiereMayorEdad: boolean;
  fechaCaducidad?: string | null;
}

@Injectable({providedIn: 'root'})
export class ProductosService {
  private http = inject(HttpClient);
  private apiUrl = 'http://localhost:5143/api/productos';
  
  // 🚦 La señal maestra que escuchará todo el sistema
  public listaProductos = signal<Producto[]>([]); 

  // 💎 NUEVO: Semáforo global para controlar el Modal desde cualquier componente
  public mostrarModal = signal(false);
  public productoAEditar = signal<Producto | null>(null);

  // 🚀 Métodos para controlar el modal globalmente
  public abrirModal(producto: Producto | null = null) {
    this.productoAEditar.set(producto);
    this.mostrarModal.set(true);
  }

  public cerrarModal() {
    this.mostrarModal.set(false);
    this.productoAEditar.set(null);
  }

  // Cargamos los productos iniciales
  async cargarProductos() {
    const data = await firstValueFrom(this.http.get<Producto[]>(this.apiUrl));
    this.listaProductos.set(data);
  }

  // Guardar y actualizar automáticamente la señal
  async crearProducto(producto: Producto) {
    const nuevo = await firstValueFrom(this.http.post<Producto>(this.apiUrl, producto));
    this.listaProductos.update(actual => [...actual, nuevo]);
    return nuevo;
  }

  // Eliminar producto
  async eliminarProducto(id: number) {
    return await firstValueFrom(
      this.http.delete<void>(`${this.apiUrl}/${id}`)
    );
  }
  // 1. MÉTODO PARA EL PUT (Actualizar)
  async actualizarProducto(producto: Producto) {
    // Usamos el ID del producto para el endpoint, tal como lo definimos en .NET
    return await firstValueFrom(
      this.http.put<void>(`${this.apiUrl}/${producto.id}`, producto)
    );
  }

  // 2. MÉTODO INTELIGENTE (El que llamarás desde el componente)
  async guardarProducto(producto: Producto) {
    if (producto.id) {
      // Si tiene ID, es una edición -> PUT
      await this.actualizarProducto(producto);
      this.listaProductos.update(lista => 
      lista.map(p => p.id === producto.id ? producto : p)
      );
    } else {
      // Si no tiene ID, es nuevo -> POST
      await this.crearProducto(producto);
    }
  }
  async cargarMasiva(productos: Producto[]) {
    // Asegúrate de que el endpoint en .NET sea /api/productos/bulk
    return await firstValueFrom(this.http.post<void>(`${this.apiUrl}/bulk`, productos));
  }
}