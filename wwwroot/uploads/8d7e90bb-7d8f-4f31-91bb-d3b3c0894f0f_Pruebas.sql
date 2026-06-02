use clientes

exec sp_help 'usuarios'
exec sp_help 'DetalleOrdenServicios'
exec sp_help 'OrdenesServicios'

alter table detalle_orden_servicio
add constraint fk_detalle_usuarios
foreign key (id_usuario) references usuarios(id_usuario)

alter table detalle_orden_servicio
add constraint fk_detalle_orden
foreign key (id_orden) references orden_servicio(id_orden)

exec sp_rename 'OrdenServicio', 'OrdenesServicios'

delete from OrdenesServicios where id_orden = 2
select * from OrdenesServicios

exec sp_rename 'detalle_orden_servicio', 'DetalleOrdenServicios'