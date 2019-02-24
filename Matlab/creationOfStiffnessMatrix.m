function K = creationOfStiffnessMatrix ()
% Computing the stiffness matrix for a brick element. 8 noded hexahedron
% element giving a 24x24 stiffness matrix. The shape functions are
% converted to natural coordinates and following the general way of
% computing the stiffness matrix.

%%%% OBS!!! lx, ly and lz is just half of the lengths of the cube. The cube
%%%% has a size of 2*lx x 2*ly x 2*lz

% k = ksi, e = eta, z = zeta
clc;

syms k real, syms e real, syms z real, syms lx real, syms ly real, syms lz real


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
    dN(1,i) = diff(N(i),k)/lx;
    dN(2,i) = diff(N(i),e)/ly;
    dN(3,i) = diff(N(i),z)/lz;
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
K = lx*ly*lz*K;

% Printing stiffness matrix to textfile that could be copied to C#
[rows, columns] = size(K);

fileID = fopen('stiffnessMatrix.txt','w');

for i=1:rows
  fprintf (fileID,'{');
  for j = 1:columns
      
     c = char(K(i,j));
     s = c;
     
     % C# dont use the notation ^2. Changed to multiplied together instead
     for k = 1: length(c)
         if (strcmp(c(k),'^'))
            if(strcmp(c(k-1),'x'))
                  s = strrep(s,'lx^2','(lx*lx)');
            elseif(strcmp(c(k-1),'y'))
                  s = strrep(s,'ly^2','(ly*ly)');
            elseif(strcmp(c(k-1),'z'))
                  s = strrep(s,'lz^2','(lz*lz)');
            end
         end
     end
     
     if (j < columns)
         fprintf(fileID,'%s, ',s);
     else
         fprintf(fileID,'%s',s);
     end
      
  end
  if(i<rows)
    fprintf (fileID,'},\n');
  else
    fprintf (fileID,'},');
  end
end

fclose(fileID);


%%%CHECK:
% Inserting values to compare with other stiffness-matrices.
E = 10;
nu = 0.3;
lx = 10;
ly = 10;
lz = 10;

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

K_show = vpa(K_show,8) %% Display matrix
