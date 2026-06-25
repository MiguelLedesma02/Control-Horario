# Guía de Despliegue SaaS

Esta guía cubre cómo subir el sistema a un entorno productivo como SaaS.

## Stack recomendado para producción

| Componente   | Servicio recomendado                          | Alternativas |
|--------------|-----------------------------------------------|--------------|
| Backend API  | Azure App Service / AWS Elastic Beanstalk     | Railway, Render, Fly.io |
| Base de datos| Azure SQL Database / AWS RDS SQL Server       | PostgreSQL en Supabase |
| Frontend SPA | Vercel / Netlify / Cloudflare Pages           | Azure Static Web Apps |
| Storage (exports) | Azure Blob Storage / AWS S3              | — |
| CDN          | Cloudflare                                    | — |

## Variables de entorno (backend)

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__Default=<conn string SQL Server>
DatabaseProvider=SqlServer
Jwt__Key=<secret de al menos 32 caracteres, generado al azar>
Jwt__Issuer=https://api.tudominio.com
Jwt__Audience=https://app.tudominio.com
Jwt__ExpiresHours=8
Cors__AllowedOrigins__0=https://app.tudominio.com
```

## Variables de entorno (frontend)

```
VITE_API_URL=https://api.tudominio.com/api
```

## Build de producción

### Backend
```bash
cd backend
dotnet publish ControlHorario.Api -c Release -o ./publish
# Subir contenido de ./publish al servicio de hosting
```

### Frontend
```bash
cd frontend
npm install
npm run build
# Subir contenido de ./dist a Vercel/Netlify/etc
```

## Migración de datos

EF Core está configurado para usar `EnsureCreated()` en desarrollo. Para producción usar migrations:

```bash
cd backend
dotnet ef migrations add Initial -p ControlHorario.Infrastructure -s ControlHorario.Api
dotnet ef database update -p ControlHorario.Infrastructure -s ControlHorario.Api
```

## Multi-tenant (extensión futura)

El modelo actual es single-tenant. Para soportar múltiples pymes:

1. Agregar entidad `Empresa` con `Id`, `RazonSocial`, `Cuit`, `PlanComercial`
2. Agregar `EmpresaId` como FK a las tablas: Usuarios, Empleados, Horarios, Feriados, ParametrosEmpresa, Cierres
3. Inyectar un `ITenantContext` que lea `EmpresaId` del JWT
4. Agregar `HasQueryFilter` global en EF Core para filtrar por tenant
5. Login: discrimar tenant por subdominio (`empresa.controlhorario.com`) o por dominio del email

## Hardening de seguridad

- [ ] Rotar el `Jwt:Key` periódicamente
- [ ] HTTPS obligatorio (HSTS habilitado)
- [ ] Rate limiting en `/api/auth/login` (ASP.NET Rate Limiting middleware)
- [ ] Logging estructurado (Serilog → Application Insights)
- [ ] Backup diario automático de la DB
- [ ] Encriptación at-rest para PII (DNI, CUIL)
- [ ] Auditoría de accesos en tabla aparte
- [ ] CSP headers en frontend
- [ ] Refresh tokens (actualmente solo access token)

## Costo estimado mensual (Azure, 10 pymes ~1000 empleados totales)

| Recurso                     | Precio aprox |
|-----------------------------|--------------|
| App Service P1v3            | USD 70       |
| Azure SQL S1                | USD 30       |
| Static Web Apps             | USD 0        |
| Storage 50GB                | USD 2        |
| Application Insights básico | USD 5        |
| **Total**                   | **~USD 110/mes** |

Con un precio de USD 5/empleado/mes esto cierra positivo desde 25 empleados pagos.
