function K = creationOfStiffnessMatrix ()
% Computing the stiffness matrix for a brick element. 8 noded hexahedron
% element giving a 24x24 stiffness matrix. The shape functions are
% converted to natural coordinates and following the general way of
% computing the stiffness matrix.

% k = ksi, e = eta, z = zeta

syms k real, syms e real, syms z real, syms a real, syms b real, syms c real


%Shape functions
N1 = 1/8*((1-k)*(1-e)*(1-z));
N2 = 1/8*((1+k)*(1-e)*(1-z));
N3 = 1/8*((1+k)*(1+e)*(1-z));
N4 = 1/8*((1-k)*(1+e)*(1-z));
N5 = 1/8*((1-k)*(1-e)*(1+z));
N6 = 1/8*((1+k)*(1-e)*(1+z));
N7 = 1/8*((1+k)*(1+e)*(1+z));
N8 = 1/8*((1-k)*(1+e)*(1+z));

N = [N1, N2, N3, N4, N5, N6, N7, N8];

dN = sym(zeros(3,8));

%Differentiation shape funcctions in natural coordinates
for i = 1 : length(N)
    dN(1,i) = diff(N(i),k)/a;
    dN(2,i) = diff(N(i),e)/b;
    dN(3,i) = diff(N(i),z)/c;
end

B = sym(zeros(6,24));

%Assemblying of B-matrix
%First 3 rows
for i = 1:3
   for j = 0:7
       B(i,3*j+1+(i-1)) = dN(i,j+1);
   end
end

%Fourth row
for i = 0:7
    B(4,3*i+1) = dN(2,i+1);
end

for i = 0:7
    B(4,3*i+2) = dN(1,i+1);
end

%Fifth row
for i = 0:7
    B(5,3*i+1) = dN(3,i+1);
end

for i = 0:7
    B(5,3*i+3) = dN(1,i+1);
end

%Sixth row
for i = 0:7
    B(6,3*i+2) = dN(3,i+1);
end

for i = 0:7
    B(6,3*i+3) = dN(2,i+1);
end


% Consttitutive matrix
syms d11, syms d12, syms d13, syms d22, syms d23, syms d33, syms d44, syms d55, syms d66
D = [
    d11, d12, d13, 0, 0, 0;
    d12, d22, d23, 0, 0, 0;
    d12, d23, d33, 0, 0, 0;
    0, 0, 0, d44, 0, 0;
    0, 0, 0, 0, d55, 0;
    0, 0, 0, 0, 0, d66;];

%Preallocating the size of the stiffness matrix.
K = zeros(24,24);

%Multiplying matrixes to assembly stiffness matrix. Just need to integrate
%to obtain the full stiffness matrix.
K = B'*D*B;

%Integrate the matriz in natural coordinates
K = int(K,k,-1,1);
K = int(K,e,-1,1);
K = int(K,z,-1,1);

%Multiply with abc for convertion to cartessian coordinates
K = a*b*c*K;


%%%CHECK:
% Inserting values to compare with other stiffness-matrices.
E = 10;
nu = 0.3;
a = 10;
b = 10;
c = 10;

%Constitutive matrix
val = E/((1+nu)*(1-2*nu));

d11 = val*(1-nu);
d44 = val*(1-2*nu)/2;
d55 = d44;
d12 = val*nu;
d22 = d11;
d23 = d12;
d33 = d11;
d13 = d12;
d66 = d44;

K_show = subs(K);

K_show = vpa(K_show,8); %% Display matrix
