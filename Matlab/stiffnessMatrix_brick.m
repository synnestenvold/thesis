function K = stiffnessMatrix_brick (E,nu,length_x,length_y,length_z)
% STIFFNESSMATRIX_BRICK Compute stiffness matrix for brick element
%   K = stiffnessMatrix_brick (E,nu,length_x,length_y,length_z) Computes
%   the 24x24 stiffness matrix for a regular 8 noded hexahedral finite 
%   element with Young´s modulus "E", Poisson ratio "nu" and lengths in x, 
%   y and z direction "length_x", "length_y" and "length_z" respectively.
%   Numerical integration is performed with 8 Gauss points, which yields
%   exact results for regular elements. Weight factors are one and 
%   therefore not included in the code.
%
%   Contact: Diego.Petraroia@rub.de
%
% Compute 3D constitutive matrix (linear continuum mechanics)
clc;
C = E./((1+nu)*(1-2*nu))*[1-nu nu nu 0 0 0; nu 1-nu nu 0 0 0;...
    nu nu 1-nu 0 0 0; 0 0 0 (1-2*nu)/2 0 0; 0 0 0 0 (1-2*nu)/2 0;...
    0 0 0 0 0 (1-2*nu)/2];
%
% Gauss points coordinates on each direction

GaussPoint = [-1/sqrt(3), 1/sqrt(3)];
%
% Matrix of vertices coordinates. Generic element centred at the origin.
coordinates = zeros(8,3);
coordinates(1,:) = [-length_x/2 -length_y/2 -length_z/2];
coordinates(2,:) = [length_x/2 -length_y/2 -length_z/2];
coordinates(3,:) = [length_x/2 length_y/2 -length_z/2];
coordinates(4,:) = [-length_x/2 length_y/2 -length_z/2];
coordinates(5,:) = [-length_x/2 -length_y/2 length_z/2];
coordinates(6,:) = [length_x/2 -length_y/2 length_z/2];
coordinates(7,:) = [length_x/2 length_y/2 length_z/2];
coordinates(8,:) = [-length_x/2 length_y/2 length_z/2];

% Preallocate memory for stiffness matrix
K = zeros (24,24);
Q = zeros(6,24);
A = zeros(3,8);
% Loop over each Gauss point
for xi1=GaussPoint
    for xi2=GaussPoint
        for xi3=GaussPoint
            % Compute shape functions derivatives
            dShape = (1/8)*[-(1-xi2)*(1-xi3),(1-xi2)*(1-xi3),...
                (1+xi2)*(1-xi3),-(1+xi2)*(1-xi3),-(1-xi2)*(1+xi3),...
                (1-xi2)*(1+xi3),(1+xi2)*(1+xi3),-(1+xi2)*(1+xi3);...
                -(1-xi1)*(1-xi3),-(1+xi1)*(1-xi3),(1+xi1)*(1-xi3),...
                (1-xi1)*(1-xi3),-(1-xi1)*(1+xi3),-(1+xi1)*(1+xi3),...
                (1+xi1)*(1+xi3),(1-xi1)*(1+xi3);-(1-xi1)*(1-xi2),...
                -(1+xi1)*(1-xi2),-(1+xi1)*(1+xi2),-(1-xi1)*(1+xi2),...
                (1-xi1)*(1-xi2),(1+xi1)*(1-xi2),(1+xi1)*(1+xi2),...
                (1-xi1)*(1+xi2)];
            % Compute Jacobian matrix
            JacobianMatrix = dShape*coordinates
            % Compute auxiliar matrix for construction of B-Operator
            auxiliar = inv(JacobianMatrix)*dShape;

            % Preallocate memory for B-Operator
            B = zeros(6,24);
            % Construct first three rows
            for i=1:3
                for j=0:7
                    B(i,3*j+1+(i-1)) = auxiliar(i,j+1);
                end
            end
            % Construct fourth row
            for j=0:7
                B(4,3*j+1) = auxiliar(2,j+1);
            end
            for j=0:7
                B(4,3*j+2) = auxiliar(1,j+1);
            end
            % Construct fifth row
            for j=0:7
                B(5,3*j+1) = auxiliar(3,j+1);
            end
            for j=0:7
                B(5,3*j+3) = auxiliar(1,j+1);
            end
            % Construct sixth row
            for j=0:7
                B(6,3*j+2) = auxiliar(3,j+1);
            end
            for j=0:7
                B(6,3*j+3) = auxiliar(2,j+1);
            end
            
            
            % Add to stiffness matrix
            K = K+B'*C*B*det(JacobianMatrix);
  
            
        end
    end
end