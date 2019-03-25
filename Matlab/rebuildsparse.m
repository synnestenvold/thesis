%%Rebuilding full matrix for sparse using the .mtx file from abaqus.

clc
clear

matrix = load('1x1x10_STIF1.mtx');
stiffness=matrix(:,5);
length(stiffness);
l=max(matrix(:,1))*3;


GlobalK=zeros(l,l);

for i =1:length(stiffness)
    a=matrix(i,1);
    b=matrix(i,2);
    c=matrix(i,3);
    d=matrix(i,4);
    
    GlobalK(3*a-(3-b),3*c-(3-d))=stiffness(i);
end
disp(GlobalK);

[n,m]=size(GlobalK);
symK=GlobalK'+GlobalK;
symK(1:n+1:end)=diag(GlobalK)


connectivity = [7,3,2,6,5,1,0,4];

K = zeros(24,24);

for i = 1: length(connectivity)
    for j = 1: length(connectivity)
        for k = 1 : 3
            for e = 1 : 3
               
                K(3*i-3 + k,3*j-3+e ) = symK(3*connectivity(i)+k, 3*connectivity(j)+e);
                
            end
            
        end
        
    end
    
end







