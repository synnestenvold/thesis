clc;
clear;

mesh1 = [5.4	29	58.2	139	256.2	492.8	717.8];
mesh2 = [6.8	32	67.2	184.6	255	496.8	732.6];

mesh = [5.4,6.8;29,32;58.2,67.2;139,184.6;256.2,255;492.8,496.8;717.8,732.6];


figure('Renderer', 'painters', 'Position', [10 10 900 500])
hold on
b = bar(mesh,'FaceColor','flat');

names = [81
375
1029
2187
3993
6591
10125
];



set(gca,'xtick',[1:7],'xticklabel',names);

set(gca, 'FontName', 'Times New Roman')
set(gca,'fontsize',14)
ylabel('Time[ms]');
xlabel('Number of dofs');

%labels=['MeshCurve';'MeshSurface'];
%legend(b,labels);

labels=['1029 doff';'1536 dofs'];
legend(b,labels);



%h = [b(1)];
 % Now call the legend function passing the handle h and specify the text
legend(b,'MeshCurve','MeshSurface','Location','north');



