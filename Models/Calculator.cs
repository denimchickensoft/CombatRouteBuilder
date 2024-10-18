namespace CRB.Models;

static class Calculator
{
    public static (double, double) ConvertToMerc(double lat, double lon, string theatre)
    {
        Maps.TransverseMercator map = Maps.Projections.ProjectionFromTheatre(theatre);
        // Given parameters
        double a = 6378137.0; // Semi-major axis for WGS84
        double f = 1 / 298.257223563; // Flattening for WGS84
        double e = Math.Sqrt(2 * f - Math.Pow(f, 2)); // Eccentricity of the ellipsoid
        double e_prime_sq = Math.Pow(e, 2) / (1 - Math.Pow(e, 2)); // Second eccentricity squared

        double k0 = map.ScaleFactor; // Scale factor
        double lambda_0 = Math.PI * map.CentralMeridian / 180; // Central meridian in radians
        double E0 = map.FalseEasting; // False easting
        double N0 = map.FalseNorthing; // False northing

        // Input coordinates
        double phi = Math.PI * lat / 180; // Latitude in radians
        double lambda_ = Math.PI * lon / 180; // Longitude in radians

        // Step-by-step calculation
        double N = a / Math.Sqrt(1 - Math.Pow(e, 2) * Math.Pow(Math.Sin(phi), 2)); // Radius of curvature in the prime vertical
        double T = Math.Pow(Math.Tan(phi), 2); // Square of tangent of latitude
        double C = e_prime_sq * Math.Pow(Math.Cos(phi), 2); // Second eccentricity component
        double A = (lambda_ - lambda_0) * Math.Cos(phi); // Longitude difference from central meridian

        // Meridian arc distance M
        double M = a * ((1 - Math.Pow(e, 2) / 4 - 3 * Math.Pow(e, 4) / 64 - 5 * Math.Pow(e, 6) / 256) * phi
                        - (3 * Math.Pow(e, 2) / 8 + 3 * Math.Pow(e, 4) / 32 + 45 * Math.Pow(e, 6) / 1024) * Math.Sin(2 * phi)
                        + (15 * Math.Pow(e, 4) / 256 + 45 * Math.Pow(e, 6) / 1024) * Math.Sin(4 * phi)
                        - 35 * Math.Pow(e, 6) / 3072 * Math.Sin(6 * phi));

        // Calculate Easting (E) and Northing (N)
        double E = E0 + k0 * N * (A + (1 - T + C) * Math.Pow(A, 3) / 6 + (5 - 18 * T + Math.Pow(T, 2) + 72 * C - 58 * e_prime_sq) * Math.Pow(A, 5) / 120);
        N = N0 + k0 * (M + N * Math.Tan(phi) * (Math.Pow(A, 2) / 2 + (5 - T + 9 * C + 4 * Math.Pow(C, 2)) * Math.Pow(A, 4) / 24
                    + (61 - 58 * T + Math.Pow(T, 2) + 600 * C - 330 * e_prime_sq) * Math.Pow(A, 6) / 720));

        //Debug.WriteLine("Northing: " + N);
        //Debug.WriteLine("Easting: " + E);
        return (N, E);
    }

    public static (double, double) ConvertToCoord(double N, double E, string theatre)
    {
        //Get transverse mercator parameters for a given map
        Maps.TransverseMercator map = Maps.Projections.ProjectionFromTheatre(theatre);
        // Given parameters
        double a = 6378137.0; // Semi-major axis for WGS84
        double f = 1 / 298.257223563; // Flattening for WGS84
        double e = Math.Sqrt(2 * f - Math.Pow(f, 2)); // Eccentricity of the ellipsoid
        double e_prime_sq = Math.Pow(e, 2) / (1 - Math.Pow(e, 2)); // Second eccentricity squared

        double k0 = map.ScaleFactor; // Scale factor
        double lambda_0 = Math.PI * map.CentralMeridian / 180; // Central meridian in radians
        double E0 = map.FalseEasting; // False easting
        double N0 = map.FalseNorthing; // False northing

        // Remove false northing and easting
        double N_corr = N - N0;
        double E_corr = E - E0;

        // Calculate meridional arc (M)
        double M = N_corr / k0;

        // Constants for the latitude computation
        double mu = M / (a * (1 - Math.Pow(e, 2) / 4 - 3 * Math.Pow(e, 4) / 64 - 5 * Math.Pow(e, 6) / 256));

        // Compute footprint latitude (phi1) using the series expansion
        double e1 = (1 - Math.Sqrt(1 - Math.Pow(e, 2))) / (1 + Math.Sqrt(1 - Math.Pow(e, 2)));

        double phi1 = mu + (3 * e1 / 2 - 27 * Math.Pow(e1, 3) / 32) * Math.Sin(2 * mu)
                        + (21 * Math.Pow(e1, 2) / 16 - 55 * Math.Pow(e1, 4) / 32) * Math.Sin(4 * mu)
                        + 151 * Math.Pow(e1, 3) / 96 * Math.Sin(6 * mu)
                        + 1097 * Math.Pow(e1, 4) / 512 * Math.Sin(8 * mu);

        // Calculate more parameters based on phi1
        double N1 = a / Math.Sqrt(1 - Math.Pow(e, 2) * Math.Pow(Math.Sin(phi1), 2));
        double T1 = Math.Pow(Math.Tan(phi1), 2);
        double C1 = e_prime_sq * Math.Pow(Math.Cos(phi1), 2);
        double R1 = a * (1 - Math.Pow(e, 2)) / Math.Pow(1 - Math.Pow(e, 2) * Math.Pow(Math.Sin(phi1), 2), 1.5);
        double D = E_corr / (N1 * k0);

        // Latitude calculation
        double phi = phi1 - N1 * Math.Tan(phi1) / R1 * (Math.Pow(D, 2) / 2
            - (5 + 3 * T1 + 10 * C1 - 4 * Math.Pow(C1, 2) - 9 * e_prime_sq) * Math.Pow(D, 4) / 24
            + (61 + 90 * T1 + 298 * C1 + 45 * Math.Pow(T1, 2) - 252 * e_prime_sq - 3 * Math.Pow(C1, 2)) * Math.Pow(D, 6) / 720);

        // Longitude calculation
        double lambda = lambda_0 + (D - (1 + 2 * T1 + C1) * Math.Pow(D, 3) / 6
                    + (5 - 2 * C1 + 28 * T1 - 3 * Math.Pow(C1, 2) + 8 * e_prime_sq + 24 * Math.Pow(T1, 2)) * Math.Pow(D, 5) / 120) / Math.Cos(phi1);

        // Convert radians to degrees
        double latitude = phi * 180 / Math.PI;
        double longitude = lambda * 180 / Math.PI;

        // Output results
        //Debug.WriteLine("Latitude: " + latitude);
        //Debug.WriteLine("Longitude: " + longitude);
        return (latitude, longitude);
    }
}